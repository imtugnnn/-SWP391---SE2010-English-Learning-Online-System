using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Helpers.Admin.AcademicYears;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AcademicYearsController : Controller
{
    private readonly AppDbContext _context;

    public AcademicYearsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var years = await _context.AcademicYears!
            .AsNoTracking()
            .Include(y => y.Classes)
            .OrderByDescending(y => y.StartDate ?? DateTime.MinValue)
            .ThenByDescending(y => y.YearLabel)
            .Select(y => new AcademicYearListItemViewModel
            {
                AcademicYearId = y.AcademicYearId,
                YearLabel = y.YearLabel,
                StartDate = y.StartDate,
                EndDate = y.EndDate,
                IsActive = y.IsActive,
                ClassCount = y.Classes.Count
            })
            .ToListAsync();

        return View("~/Views/Admin/AcademicYears/Index.cshtml", years);
    }

    public IActionResult Create()
    {
        return View("~/Views/Admin/AcademicYears/Create.cshtml", new AcademicYearCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AcademicYearCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return View("~/Views/Admin/AcademicYears/Create.cshtml", vm);
        }

        if (vm.StartDate.HasValue && vm.EndDate.HasValue && vm.StartDate.Value >= vm.EndDate.Value)
        {
            ModelState.AddModelError(nameof(vm.EndDate), "Ngày kết thúc phải sau ngày bắt đầu.");
            return View("~/Views/Admin/AcademicYears/Create.cshtml", vm);
        }

        var exists = await _context.AcademicYears!.AnyAsync(y => y.YearLabel == vm.YearLabel.Trim());
        if (exists)
        {
            ModelState.AddModelError(nameof(vm.YearLabel), "Năm học này đã tồn tại.");
            return View("~/Views/Admin/AcademicYears/Create.cshtml", vm);
        }

        var hasActiveYear = await _context.AcademicYears!.AnyAsync(y => y.IsActive);
        var isActive = vm.IsActive || !hasActiveYear;

        if (isActive)
        {
            await DeactivateAllAcademicYearsAsync();
        }

        var academicYear = new AcademicYear
        {
            YearLabel = vm.YearLabel.Trim(),
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            IsActive = isActive
        };

        _context.AcademicYears!.Add(academicYear);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = academicYear.AcademicYearId });
    }

    public async Task<IActionResult> Edit(int id, int? selectedClassId)
    {
        var vm = await BuildEditViewModel(id, selectedClassId);
        if (vm == null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/AcademicYears/Edit.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddClass(int id, AcademicYearEditViewModel vm)
    {
        var academicYear = await _context.AcademicYears!.FirstOrDefaultAsync(y => y.AcademicYearId == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        var teacher = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == vm.NewClass.TeacherId);

        if (teacher == null || teacher.Role?.Name != "Teacher")
        {
            ModelState.AddModelError("NewClass.TeacherId", "Vui lòng chọn một giáo viên chủ nhiệm hợp lệ.");
        }

        if (vm.NewClass.ClassName.Contains('\n') || vm.NewClass.ClassName.Contains('\r'))
        {
            ModelState.AddModelError("NewClass.ClassName", "Tên lớp học không được chứa ký tự xuống dòng.");
        }

        if (vm.NewClass.GradeLevel.Contains('\n') || vm.NewClass.GradeLevel.Contains('\r'))
        {
            ModelState.AddModelError("NewClass.GradeLevel", "Khối lớp không được chứa ký tự xuống dòng.");
        }

        var className = vm.NewClass.ClassName.Trim();
        var existingClass = await _context.Classes!
            .AsNoTracking()
            .AnyAsync(c => c.AcademicYearId == id && c.ClassName == className);

        if (existingClass)
        {
            ModelState.AddModelError("NewClass.ClassName", "Tên lớp học phải là duy nhất trong cùng một năm học.");
        }

        var parsedEmails = ParseEmails(vm.NewClass.StudentEmails, preserveOrder: true);
        var duplicateEmails = parsedEmails
            .GroupBy(email => email, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        var emails = parsedEmails
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (emails.Count == 0)
        {
            ModelState.AddModelError("NewClass.StudentEmails", "Vui lòng nhập ít nhất một email học sinh.");
        }

        if (duplicateEmails.Count > 0)
        {
            ModelState.AddModelError("NewClass.StudentEmails", $"Không được phép trùng lặp email học sinh: {string.Join(", ", duplicateEmails)}.");
        }

        var students = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => emails.Contains(u.Email))
            .ToListAsync();

        var missingEmails = emails
            .Except(students.Select(s => s.Email), StringComparer.OrdinalIgnoreCase)
            .ToList();

        var invalidStudents = students
            .Where(s => s.Role?.Name != "Student")
            .Select(s => s.Email)
            .ToList();

        if (missingEmails.Count > 0)
        {
            ModelState.AddModelError("NewClass.StudentEmails", $"Email học sinh không tồn tại trên hệ thống: {string.Join(", ", missingEmails)}.");
        }

        if (invalidStudents.Count > 0)
        {
            ModelState.AddModelError("NewClass.StudentEmails", $"Các tài khoản này không phải là học sinh: {string.Join(", ", invalidStudents)}.");
        }

        if (!ModelState.IsValid)
        {
            var invalidVm = await BuildEditViewModel(id, vm.SelectedClassId, vm.NewClass);
            if (invalidVm == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Edit.cshtml", invalidVm);
        }

        var newClass = new Class
        {
            AcademicYearId = academicYear.AcademicYearId,
            ClassName = className,
            GradeLevel = vm.NewClass.GradeLevel.Trim(),
            TeacherId = teacher!.Id
        };

        _context.Classes!.Add(newClass);
        await _context.SaveChangesAsync();

        var enrollments = students
            .Where(s => s.Role?.Name == "Student")
            .Select(s => new ClassEnrollment
            {
                ClassId = newClass.ClassId,
                StudentId = s.Id
            })
            .ToList();

        _context.ClassEnrollments!.AddRange(enrollments);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id = academicYear.AcademicYearId });
    }

    [HttpGet]
    public IActionResult DownloadClassTemplate(int id)
    {
        var templateBytes = AcademicYearExcelHelper.CreateTemplate();
        var fileName = $"academic-year-{id}-student-template.xlsx";
        return File(
            templateBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoadStudentsFromExcel(int id, AcademicYearEditViewModel vm)
    {
        var academicYear = await _context.AcademicYears!.FirstOrDefaultAsync(y => y.AcademicYearId == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        ModelState.Remove("NewClass.ClassName");
        ModelState.Remove("NewClass.GradeLevel");
        ModelState.Remove("NewClass.TeacherId");
        ModelState.Remove("NewClass.StudentEmails");
        ModelState.Remove("SelectedClassId");

        if (vm.ImportFile == null || vm.ImportFile.Length == 0)
        {
            ModelState.AddModelError(nameof(vm.ImportFile), "Vui lòng chọn tệp Excel.");
            var invalidVm = await BuildEditViewModel(id, vm.SelectedClassId, vm.NewClass);
            if (invalidVm == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Edit.cshtml", invalidVm);
        }

        var extension = Path.GetExtension(vm.ImportFile.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            ModelState.AddModelError(nameof(vm.ImportFile), "Chỉ hỗ trợ định dạng tệp .xlsx.");
            var invalidVm = await BuildEditViewModel(id, vm.SelectedClassId, vm.NewClass);
            if (invalidVm == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Edit.cshtml", invalidVm);
        }

        List<ExcelStudentImportRow> rows;
        await using (var stream = vm.ImportFile.OpenReadStream())
        {
            rows = AcademicYearExcelHelper.ReadRows(stream);
        }

        if (rows.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Tệp Excel không chứa bất kỳ email học sinh nào.");
            var invalidVm = await BuildEditViewModel(id, vm.SelectedClassId, vm.NewClass);
            if (invalidVm == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Edit.cshtml", invalidVm);
        }

        var excelEmails = rows.Select(r => r.StudentEmail.Trim()).ToList();

        var usersInExcel = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => excelEmails.Contains(u.Email))
            .ToListAsync();

        var emails = new List<string>();
        var errors = new List<string>();
        var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var row in rows)
        {
            var email = row.StudentEmail.Trim();
            if (!seenEmails.Add(email))
            {
                errors.Add($"Dòng {row.RowNumber}: Email học sinh trùng lặp '{email}' không được chấp nhận.");
                continue;
            }

            var user = usersInExcel.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase));
            if (user == null)
            {
                errors.Add($"Dòng {row.RowNumber}: Không tìm thấy học sinh '{email}' trên hệ thống.");
                continue;
            }

            if (user.Role?.Name != "Student")
            {
                errors.Add($"Dòng {row.RowNumber}: Tài khoản '{email}' không phải là tài khoản học sinh.");
                continue;
            }

            emails.Add(email);
        }

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            vm.NewClass.StudentEmails = string.Join(Environment.NewLine, emails);
            var invalidVm = await BuildEditViewModel(id, vm.SelectedClassId, vm.NewClass);
            if (invalidVm == null)
            {
                return NotFound();
            }

            return View("~/Views/Admin/AcademicYears/Edit.cshtml", invalidVm);
        }

        vm.NewClass.StudentEmails = string.Join(Environment.NewLine, emails);

        var updatedVm = await BuildEditViewModel(id, vm.SelectedClassId, vm.NewClass);
        if (updatedVm == null)
        {
            return NotFound();
        }

        return View("~/Views/Admin/AcademicYears/Edit.cshtml", updatedVm);
    }

    private async Task<AcademicYearEditViewModel?> BuildEditViewModel(int id, int? selectedClassId = null, AddClassViewModel? newClass = null)
    {
        var academicYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.AcademicYearId == id);

        if (academicYear == null)
        {
            return null;
        }

        var classes = await _context.Classes!
            .AsNoTracking()
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .Where(c => c.AcademicYearId == id)
            .OrderBy(c => c.ClassName)
            .ToListAsync();

        var vm = new AcademicYearEditViewModel
        {
            AcademicYearId = academicYear.AcademicYearId,
            YearLabel = academicYear.YearLabel,
            StartDate = academicYear.StartDate,
            EndDate = academicYear.EndDate,
            IsActive = academicYear.IsActive,
            SelectedClassId = selectedClassId,
            Classes = classes.Select(c => new ClassSummaryViewModel
            {
                ClassId = c.ClassId,
                ClassName = c.ClassName,
                GradeLevel = c.GradeLevel,
                TeacherName = c.Teacher?.Username ?? "Unassigned",
                TeacherEmail = c.Teacher?.Email ?? string.Empty,
                StudentEmails = c.Enrollments
                    .Select(e => e.Student.Email)
                    .OrderBy(email => email)
                    .ToList()
            }).ToList(),
            NewClass = newClass ?? new AddClassViewModel()
        };

        if (selectedClassId.HasValue)
        {
            vm.SelectedClass = vm.Classes.FirstOrDefault(c => c.ClassId == selectedClassId.Value);
            vm.SelectedClassId = vm.SelectedClass?.ClassId;
        }

        await PopulateTeacherOptionsAsync(vm.NewClass.TeacherId);
        return vm;
    }

    private async Task PopulateTeacherOptionsAsync(int? selectedTeacherId)
    {
        var teachers = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role != null && u.Role.Name == "Teacher")
            .OrderBy(u => u.Username)
            .ToListAsync();

        ViewBag.TeacherId = new SelectList(teachers, "Id", "Email", selectedTeacherId);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id)
    {
        var academicYear = await _context.AcademicYears!.FirstOrDefaultAsync(y => y.AcademicYearId == id);
        if (academicYear == null)
        {
            return NotFound();
        }

        await DeactivateAllAcademicYearsAsync();
        academicYear.IsActive = true;
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveClass(int id, int classId)
    {
        var classEntity = await _context.Classes!
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.ClassId == classId && c.AcademicYearId == id);

        if (classEntity != null)
        {
            if (classEntity.Enrollments.Any())
            {
                _context.ClassEnrollments!.RemoveRange(classEntity.Enrollments);
            }
            _context.Classes!.Remove(classEntity);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Edit), new { id = id });
    }


    private async Task DeactivateAllAcademicYearsAsync()
    {
        var activeYears = await _context.AcademicYears!
            .Where(y => y.IsActive)
            .ToListAsync();

        foreach (var activeYear in activeYears)
        {
            activeYear.IsActive = false;
        }
    }

    private static List<string> ParseEmails(string? input, bool preserveOrder = false)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return new List<string>();
        }

        var emails = input
            .Split(new[] { '\r', '\n', ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(email => email.Trim())
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .ToList();

        return preserveOrder
            ? emails
            : emails.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}
