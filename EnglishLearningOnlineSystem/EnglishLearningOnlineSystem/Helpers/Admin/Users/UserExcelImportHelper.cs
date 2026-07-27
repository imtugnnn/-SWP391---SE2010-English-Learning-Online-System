using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml.Linq;

namespace EnglishLearningOnlineSystem.Helpers.Admin.Users;

public sealed record ExcelUserImportRow(
    int RowNumber,
    string Username,
    string Email,
    DateTime? BirthDate,
    string ClassName);

public static class UserExcelImportHelper
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

    public static List<ExcelUserImportRow> ReadRows(Stream xlsxStream)
    {
        using var archive = new ZipArchive(xlsxStream, ZipArchiveMode.Read, leaveOpen: true);

        var sharedStrings = ReadSharedStrings(archive);
        var dateStyleIndexes = ReadDateStyleIndexes(archive);
        var sheet = archive.GetEntry("xl/worksheets/sheet1.xml");
        if (sheet == null)
        {
            throw new InvalidOperationException("Cannot find worksheet in Excel file.");
        }

        using var sheetStream = sheet.Open();
        var sheetDoc = XDocument.Load(sheetStream);

        var rows = new List<ExcelUserImportRow>();
        foreach (var row in sheetDoc.Root?.Element(MainNs + "sheetData")?.Elements(MainNs + "row") ?? Enumerable.Empty<XElement>())
        {
            var rowNumber = (int?)row.Attribute("r") ?? 0;
            if (rowNumber <= 1)
            {
                continue;
            }

            var username = ReadTextCell(row, "A", sharedStrings);
            var email = ReadTextCell(row, "B", sharedStrings);
            var birthDate = ReadDateCell(row, "C", sharedStrings, dateStyleIndexes);
            var className = ReadTextCell(row, "D", sharedStrings);

            if (string.IsNullOrWhiteSpace(username)
                && string.IsNullOrWhiteSpace(email)
                && string.IsNullOrWhiteSpace(className)
                && !birthDate.HasValue)
            {
                continue;
            }

            rows.Add(new ExcelUserImportRow(
                rowNumber,
                username.Trim(),
                email.Trim(),
                birthDate,
                className.Trim()));
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

    private static HashSet<int> ReadDateStyleIndexes(ZipArchive archive)
    {
        var entry = archive.GetEntry("xl/styles.xml");
        if (entry == null)
        {
            return new HashSet<int>();
        }

        using var stream = entry.Open();
        var doc = XDocument.Load(stream);

        var knownDateFormatIds = new HashSet<int>
        {
            14, 15, 16, 17, 18, 19, 20, 21, 22, 27, 28, 29, 30, 31, 32, 45, 46, 47,
            50, 57, 58, 165, 166, 167, 168, 169, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180, 181, 182, 183
        };

        var customDateFormatIds = doc.Root?
            .Element(MainNs + "numFmts")?
            .Elements(MainNs + "numFmt")
            .Where(numFmt =>
            {
                var formatCode = ((string?)numFmt.Attribute("formatCode")) ?? string.Empty;
                var normalized = formatCode.ToLowerInvariant();
                return normalized.Contains("yy") || normalized.Contains("dd") || normalized.Contains("mm") || normalized.Contains("hh") || normalized.Contains("ss");
            })
            .Select(numFmt => (int?)numFmt.Attribute("numFmtId"))
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet() ?? new HashSet<int>();

        foreach (var id in customDateFormatIds)
        {
            knownDateFormatIds.Add(id);
        }

        var styles = new HashSet<int>();
        var cellXfs = doc.Root?.Element(MainNs + "cellXfs")?.Elements(MainNs + "xf") ?? Enumerable.Empty<XElement>();
        var index = 0;
        foreach (var xf in cellXfs)
        {
            var numFmtId = (int?)xf.Attribute("numFmtId") ?? 0;
            if (knownDateFormatIds.Contains(numFmtId))
            {
                styles.Add(index);
            }

            index++;
        }

        return styles;
    }

    private static string ReadTextCell(XElement row, string column, IReadOnlyList<string> sharedStrings)
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

    private static DateTime? ReadDateCell(XElement row, string column, IReadOnlyList<string> sharedStrings, ISet<int> dateStyleIndexes)
    {
        var cell = row.Elements(MainNs + "c")
            .FirstOrDefault(c => string.Equals(GetColumnName((string?)c.Attribute("r")), column, StringComparison.OrdinalIgnoreCase));

        if (cell == null)
        {
            return null;
        }

        var cellType = (string?)cell.Attribute("t");
        var rawValue = ReadRawCellValue(cell, sharedStrings, cellType);
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return null;
        }

        var styleIndex = (int?)cell.Attribute("s");
        if (styleIndex.HasValue && dateStyleIndexes.Contains(styleIndex.Value) && double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var oaDate))
        {
            return DateTime.FromOADate(oaDate).Date;
        }

        var parsedFormats = new[]
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
            "yyyy-MM-dd",
            "MM/dd/yyyy",
            "M/d/yyyy"
        };

        if (DateTime.TryParseExact(rawValue.Trim(), parsedFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedExact))
        {
            return parsedExact.Date;
        }

        if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsedCulture))
        {
            return parsedCulture.Date;
        }

        if (double.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out var fallbackOaDate) && fallbackOaDate > 0 && fallbackOaDate < 60000)
        {
            return DateTime.FromOADate(fallbackOaDate).Date;
        }

        return null;
    }

    private static string ReadRawCellValue(XElement cell, IReadOnlyList<string> sharedStrings, string? cellType)
    {
        var value = cell.Element(MainNs + "v")?.Value ?? string.Empty;

        if (string.Equals(cellType, "inlineStr", StringComparison.OrdinalIgnoreCase))
        {
            return cell.Element(MainNs + "is")?.Descendants(MainNs + "t").Select(t => t.Value).FirstOrDefault() ?? string.Empty;
        }

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
      <c r="A1" t="inlineStr"><is><t>username</t></is></c>
      <c r="B1" t="inlineStr"><is><t>email</t></is></c>
      <c r="C1" t="inlineStr"><is><t>ngay_sinh</t></is></c>
      <c r="D1" t="inlineStr"><is><t>lop</t></is></c>
    </row>
    <row r="2">
      <c r="A2" t="inlineStr"><is><t>student01</t></is></c>
      <c r="B2" t="inlineStr"><is><t>student01@example.com</t></is></c>
      <c r="C2" t="inlineStr"><is><t>27/01/2015</t></is></c>
      <c r="D2" t="inlineStr"><is><t>10A1</t></is></c>
    </row>
    <row r="3">
      <c r="A3" t="inlineStr"><is><t>student02</t></is></c>
      <c r="B3" t="inlineStr"><is><t>student02@example.com</t></is></c>
      <c r="C3" t="inlineStr"><is><t>27/01/2016</t></is></c>
      <c r="D3" t="inlineStr"><is><t>10A2</t></is></c>
    </row>
  </sheetData>
</worksheet>
""";
}
