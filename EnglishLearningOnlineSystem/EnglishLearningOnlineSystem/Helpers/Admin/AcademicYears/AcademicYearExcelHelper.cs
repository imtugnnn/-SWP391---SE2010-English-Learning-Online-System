using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace EnglishLearningOnlineSystem.Helpers.Admin.AcademicYears;

public sealed record ExcelStudentImportRow(
    int RowNumber,
    string StudentEmail);

public static class AcademicYearExcelHelper
{
    private static readonly XNamespace MainNs = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

    public static byte[] CreateTemplate()
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, "[Content_Types].xml", BuildContentTypes());
            WriteEntry(archive, "_rels/.rels", BuildRootRels());
            WriteEntry(archive, "xl/workbook.xml", BuildWorkbookXml());
            WriteEntry(archive, "xl/_rels/workbook.xml.rels", BuildWorkbookRels());
            WriteEntry(archive, "xl/styles.xml", BuildStylesXml());
            WriteEntry(archive, "xl/worksheets/sheet1.xml", BuildTemplateSheetXml());
        }

        return ms.ToArray();
    }

    public static List<ExcelStudentImportRow> ReadRows(Stream xlsxStream)
    {
        using var archive = new ZipArchive(xlsxStream, ZipArchiveMode.Read, leaveOpen: true);

        var sharedStrings = ReadSharedStrings(archive);
        var sheet = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheet == null)
        {
            throw new InvalidOperationException("Cannot find worksheet in Excel file.");
        }

        using var sheetStream = sheet.Open();
        var sheetDoc = XDocument.Load(sheetStream);

        var rows = new List<ExcelStudentImportRow>();

        foreach (var row in sheetDoc.Root?.Element(MainNs + "sheetData")?.Elements(MainNs + "row") ?? Enumerable.Empty<XElement>())
        {
            var rowNumber = (int?)row.Attribute("r") ?? 0;
            if (rowNumber <= 1)
            {
                continue;
            }

            var studentEmail = ReadCell(row, "B", sharedStrings);
            if (string.IsNullOrWhiteSpace(studentEmail))
            {
                continue;
            }

            rows.Add(new ExcelStudentImportRow(rowNumber, studentEmail.Trim()));
        }

        return rows;
    }

    private static List<string> ReadSharedStrings(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/sharedStrings.xml");
        if (entry == null)
        {
            return new List<string>();
        }

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        return doc.Root?
            .Elements(MainNs + "si")
            .Select(si => string.Concat(si.Descendants(MainNs + "t").Select(t => t.Value)))
            .ToList()
            ?? new List<string>();
    }

    private static string ReadCell(XElement row, string column, IReadOnlyList<string> sharedStrings)
    {
        var cell = row.Elements(MainNs + "c")
            .FirstOrDefault(c => string.Equals(GetColumnName((string?)c.Attribute("r")), column, StringComparison.OrdinalIgnoreCase));

        if (cell == null)
        {
            return string.Empty;
        }

        var cellType = (string?)cell.Attribute("t");
        if (string.Equals(cellType, "inlineStr", StringComparison.OrdinalIgnoreCase))
        {
            return cell.Element(MainNs + "is")?.Descendants(MainNs + "t").Select(t => t.Value).FirstOrDefault() ?? string.Empty;
        }

        var value = cell.Element(MainNs + "v")?.Value ?? string.Empty;
        if (string.Equals(cellType, "s", StringComparison.OrdinalIgnoreCase) && int.TryParse(value, out var sharedIndex))
        {
            return sharedIndex >= 0 && sharedIndex < sharedStrings.Count ? sharedStrings[sharedIndex] : string.Empty;
        }

        return value;
    }

    private static string GetColumnName(string? cellReference)
    {
        if (string.IsNullOrWhiteSpace(cellReference))
        {
            return string.Empty;
        }

        var letters = new string(cellReference.TakeWhile(char.IsLetter).ToArray());
        return letters.ToUpperInvariant();
    }

    private static void WriteEntry(ZipArchive archive, string path, string content)
    {
        var entry = archive.CreateEntry(path, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }

    private static string BuildContentTypes() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml" />
  <Default Extension="xml" ContentType="application/xml" />
  <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml" />
  <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml" />
  <Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml" />
</Types>
""";

    private static string BuildRootRels() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml" />
</Relationships>
""";

    private static string BuildWorkbookXml() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"
          xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
  <sheets>
    <sheet name="Students" sheetId="1" r:id="rId1" />
  </sheets>
</workbook>
""";

    private static string BuildWorkbookRels() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml" />
</Relationships>
""";

    private static string BuildStylesXml() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <fonts count="1">
    <font>
      <sz val="11" />
      <color theme="1" />
      <name val="Calibri" />
      <family val="2" />
    </font>
  </fonts>
  <fills count="1">
    <fill>
      <patternFill patternType="none" />
    </fill>
  </fills>
  <borders count="1">
    <border>
      <left />
      <right />
      <top />
      <bottom />
      <diagonal />
    </border>
  </borders>
  <cellStyleXfs count="1">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" />
  </cellStyleXfs>
  <cellXfs count="1">
    <xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0" />
  </cellXfs>
</styleSheet>
""";

    private static string BuildTemplateSheetXml() => """
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
  <sheetData>
    <row r="1">
      <c r="A1" t="inlineStr"><is><t>No</t></is></c>
      <c r="B1" t="inlineStr"><is><t>Student Email</t></is></c>
    </row>
    <row r="2">
      <c r="A2"><v>1</v></c>
      <c r="B2" t="inlineStr"><is><t>student1@example.com</t></is></c>
    </row>
    <row r="3">
      <c r="A3"><v>2</v></c>
      <c r="B3" t="inlineStr"><is><t>student2@example.com</t></is></c>
    </row>
  </sheetData>
</worksheet>
""";
}
