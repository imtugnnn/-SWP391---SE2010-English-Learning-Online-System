//Created by TungDPL
//Created at 7/28/2026
using EnglishLearningOnlineSystem.Helpers.Admin.AcademicYears;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Admin.AcademicYears;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class AcademicYearService : IAcademicYearService
{
    private readonly IAcademicYearRepository _academicYearRepository;

    public AcademicYearService(IAcademicYearRepository academicYearRepository)
    {
        _academicYearRepository = academicYearRepository;
    }

    public async Task<List<AcademicYearListItemViewModel>> GetAcademicYearsAsync()
    {
        var years = await _academicYearRepository.GetAcademicYearsAsync();

        return years
            .OrderByDescending(y => y.StartDate ?? DateTime.MinValue)
            .ThenByDescending(y => y.YearLabel)
            .Select(y => new AcademicYearListItemViewModel
            {
                AcademicYearId = y.AcademicYearId,
                YearLabel = y.YearLabel,
                StartDate = y.StartDate,
                EndDate = y.EndDate,
                IsActive = y.IsActive,
                ClassCount = y.Classes.Count(c => !c.IsDeleted)
            })
            .ToList();
    }

    public Task<AcademicYearCreateViewModel> GetCreateViewModelAsync()
    {
        return Task.FromResult(new AcademicYearCreateViewModel());
    }

    public async Task<SelectList> GetTeacherSelectListAsync(int? selectedTeacherId)
    {
        var teachers = await _academicYearRepository.GetTeachersAsync();
        return new SelectList(teachers, "Id", "Email", selectedTeacherId);
    }

    public async Task<AcademicYearCreateResult> CreateAsync(AcademicYearCreateViewModel vm, int? adminId)
    {
        var result = new AcademicYearCreateResult();
        var trimmedLabel = vm.YearLabel?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedLabel))
        {
            result.Errors.Add(new AcademicYearValidationError(nameof(vm.YearLabel), "Tên năm học không được để trống."));
            return result;
        }

        if (vm.StartDate.HasValue && vm.EndDate.HasValue && vm.StartDate.Value >= vm.EndDate.Value)
        {
            result.Errors.Add(new AcademicYearValidationError(nameof(vm.EndDate), "Ngày kết thúc phải sau ngày bắt đầu."));
            return result;
        }

        if (await _academicYearRepository.AcademicYearLabelExistsAsync(trimmedLabel))
        {
            result.Errors.Add(new AcademicYearValidationError(nameof(vm.YearLabel), "Năm học này đã tồn tại."));
            return result;
        }

        var hasActiveYear = await _academicYearRepository.HasActiveAcademicYearAsync();
        var isActive = vm.IsActive || !hasActiveYear;

        if (isActive)
        {
            var activeYears = await _academicYearRepository.GetActiveAcademicYearsAsync();
            foreach (var activeYear in activeYears)
            {
                activeYear.IsActive = false;
            }
        }

        var academicYear = new AcademicYear
        {
            YearLabel = trimmedLabel,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            IsActive = isActive
        };

        await _academicYearRepository.AddAcademicYearAsync(academicYear);
        await _academicYearRepository.SaveChangesAsync();

        result.Success = true;
        result.AcademicYearId = academicYear.AcademicYearId;
        return result;
    }

    public async Task<AcademicYearEditResult> GetEditViewModelAsync(int id, int? selectedClassId = null, AddClassViewModel? newClass = null)
    {
        var academicYear = await _academicYearRepository.GetAcademicYearByIdAsync(id);
        if (academicYear == null)
        {
            return new AcademicYearEditResult { NotFound = true };
        }

        var classes = await _academicYearRepository.GetClassesByAcademicYearIdAsync(id);

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
                StudentEmails = c.Enrollments.Select(e => e.Student.Email).OrderBy(email => email).ToList(),
                IsDeleted = c.IsDeleted
            }).ToList(),
            NewClass = newClass ?? new AddClassViewModel()
        };

        if (selectedClassId.HasValue)
        {
            vm.SelectedClass = vm.Classes.FirstOrDefault(c => c.ClassId == selectedClassId.Value);
            vm.SelectedClassId = vm.SelectedClass?.ClassId;
        }

        return new AcademicYearEditResult
        {
            Success = true,
            ViewModel = vm
        };
    }

    public async Task<AcademicYearEditResult> AddClassAsync(int id, AcademicYearEditViewModel vm, int? adminId)
    {
        vm.NewClass ??= new AddClassViewModel();

        var academicYear = await _academicYearRepository.GetAcademicYearByIdAsync(id);
        if (academicYear == null)
        {
            return new AcademicYearEditResult { NotFound = true };
        }

        var result = new AcademicYearEditResult();
        var classInput = vm.NewClass;
        var className = classInput.ClassName?.Trim() ?? string.Empty;
        var gradeLevel = classInput.GradeLevel?.Trim() ?? string.Empty;

        var teacher = (await _academicYearRepository.GetTeachersAsync())
            .FirstOrDefault(t => t.Id == classInput.TeacherId);

        if (classInput.TeacherId == null)
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.TeacherId", "Vui lòng chọn giáo viên chủ nhiệm."));
        }
        else if (teacher == null || teacher.Role?.Name != "Teacher")
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.TeacherId", "Vui lòng chọn một giáo viên chủ nhiệm hợp lệ."));
        }

        if (string.IsNullOrWhiteSpace(className))
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.ClassName", "Vui lòng nhập tên lớp học."));
        }
        else if (className.Contains('\n') || className.Contains('\r'))
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.ClassName", "Tên lớp học không được chứa ký tự xuống dòng."));
        }

        if (string.IsNullOrWhiteSpace(gradeLevel))
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.GradeLevel", "Vui lòng nhập khối lớp."));
        }
        else if (gradeLevel.Contains('\n') || gradeLevel.Contains('\r'))
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.GradeLevel", "Khối lớp không được chứa ký tự xuống dòng."));
        }

        if (!string.IsNullOrWhiteSpace(className) && await _academicYearRepository.ClassNameExistsAsync(id, className))
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.ClassName", "Tên lớp học phải là duy nhất trong cùng một năm học."));
        }

        var parsedEmails = ParseEmails(classInput.StudentEmails, preserveOrder: true);
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
            result.Errors.Add(new AcademicYearValidationError("NewClass.StudentEmails", "Vui lòng nhập ít nhất một email học sinh."));
        }

        if (duplicateEmails.Count > 0)
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.StudentEmails", $"Không được phép trùng lặp email học sinh: {string.Join(", ", duplicateEmails)}."));
        }

        var students = await _academicYearRepository.GetUsersByEmailsAsync(emails);

        var missingEmails = emails
            .Except(students.Select(s => s.Email), StringComparer.OrdinalIgnoreCase)
            .ToList();

        var invalidStudents = students
            .Where(s => s.Role?.Name != "Student")
            .Select(s => s.Email)
            .ToList();

        if (missingEmails.Count > 0)
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.StudentEmails", $"Email học sinh không tồn tại trên hệ thống: {string.Join(", ", missingEmails)}."));
        }

        if (invalidStudents.Count > 0)
        {
            result.Errors.Add(new AcademicYearValidationError("NewClass.StudentEmails", $"Các tài khoản này không phải là học sinh: {string.Join(", ", invalidStudents)}."));
        }

        if (result.Errors.Count > 0)
        {
            result.ViewModel = (await GetEditViewModelAsync(id, vm.SelectedClassId, classInput)).ViewModel;
            return result;
        }

        var newClassEntity = new Class
        {
            AcademicYearId = academicYear.AcademicYearId,
            ClassName = className,
            GradeLevel = gradeLevel,
            TeacherId = teacher!.Id
        };

        await _academicYearRepository.AddClassAsync(newClassEntity);
        await _academicYearRepository.SaveChangesAsync();

        var enrollments = students
            .Where(s => s.Role?.Name == "Student")
            .Select(s => new ClassEnrollment
            {
                ClassId = newClassEntity.ClassId,
                StudentId = s.Id
            })
            .ToList();

        await _academicYearRepository.AddClassEnrollmentsAsync(enrollments);
        await _academicYearRepository.SaveChangesAsync();

        result.Success = true;
        result.SuccessMessage = "Thêm lớp học thành công.";
        result.ViewModel = (await GetEditViewModelAsync(id, null, new AddClassViewModel())).ViewModel;
        return result;
    }

    public async Task<AcademicYearEditResult> LoadStudentsFromExcelAsync(int id, AcademicYearEditViewModel vm)
    {
        vm.NewClass ??= new AddClassViewModel();

        var academicYear = await _academicYearRepository.GetAcademicYearByIdAsync(id);
        if (academicYear == null)
        {
            return new AcademicYearEditResult { NotFound = true };
        }

        var result = new AcademicYearEditResult();
        var modelErrors = new List<AcademicYearValidationError>();

        if (vm.ImportFile == null || vm.ImportFile.Length == 0)
        {
            modelErrors.Add(new AcademicYearValidationError(nameof(vm.ImportFile), "Vui lòng chọn tệp Excel."));
        }
        else if (!string.Equals(Path.GetExtension(vm.ImportFile.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            modelErrors.Add(new AcademicYearValidationError(nameof(vm.ImportFile), "Chỉ hỗ trợ định dạng tệp .xlsx."));
        }

        if (modelErrors.Count > 0)
        {
            result.Errors.AddRange(modelErrors);
            result.ViewModel = (await GetEditViewModelAsync(id, vm.SelectedClassId, vm.NewClass)).ViewModel;
            return result;
        }

        List<ExcelStudentImportRow> rows;
        try
        {
            await using var stream = vm.ImportFile!.OpenReadStream();
            rows = AcademicYearExcelHelper.ReadRows(stream);
        }
        catch (Exception)
        {
            result.Errors.Add(new AcademicYearValidationError(string.Empty, "Không thể đọc tệp Excel. Vui lòng kiểm tra lại định dạng file .xlsx và thử lại."));
            result.ViewModel = (await GetEditViewModelAsync(id, vm.SelectedClassId, vm.NewClass)).ViewModel;
            return result;
        }

        if (rows.Count == 0)
        {
            result.Errors.Add(new AcademicYearValidationError(string.Empty, "Tệp Excel không chứa bất kỳ email học sinh nào."));
            result.ViewModel = (await GetEditViewModelAsync(id, vm.SelectedClassId, vm.NewClass)).ViewModel;
            return result;
        }

        var excelEmails = rows
            .Select(r => r.StudentEmail.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        var usersInExcel = await _academicYearRepository.GetUsersByEmailsAsync(excelEmails);

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

        vm.NewClass.StudentEmails = string.Join(Environment.NewLine, emails);

        if (errors.Count > 0)
        {
            foreach (var error in errors)
            {
                result.Errors.Add(new AcademicYearValidationError(string.Empty, error));
            }

            result.ViewModel = (await GetEditViewModelAsync(id, vm.SelectedClassId, vm.NewClass)).ViewModel;
            return result;
        }

        result.Success = true;
        result.ViewModel = (await GetEditViewModelAsync(id, vm.SelectedClassId, vm.NewClass)).ViewModel;
        return result;
    }

    public async Task<AcademicYearActionResult> SetActiveAsync(int id, int? adminId)
    {
        var academicYear = await _academicYearRepository.GetAcademicYearByIdAsync(id);
        if (academicYear == null)
        {
            return new AcademicYearActionResult { NotFound = true };
        }

        var activeYears = await _academicYearRepository.GetActiveAcademicYearsAsync();
        foreach (var activeYear in activeYears)
        {
            activeYear.IsActive = false;
        }

        academicYear.IsActive = true;
        await _academicYearRepository.SaveChangesAsync();

        return new AcademicYearActionResult
        {
            Success = true,
            SuccessMessage = "Kích hoạt năm học thành công."
        };
    }

    public async Task<AcademicYearActionResult> RemoveClassAsync(int id, int classId, int? adminId)
    {
        var classEntity = await _academicYearRepository.GetClassByIdAsync(classId);
        if (classEntity == null || classEntity.AcademicYearId != id)
        {
            return new AcademicYearActionResult { NotFound = true };
        }

        classEntity.IsDeleted = true;
        await _academicYearRepository.SaveChangesAsync();

        return new AcademicYearActionResult
        {
            Success = true,
            SuccessMessage = $"Lớp học '{classEntity.ClassName}' đã được xóa thành công."
        };
    }

    public async Task<AcademicYearActionResult> RestoreClassAsync(int id, int classId, int? adminId)
    {
        var classEntity = await _academicYearRepository.GetClassByIdAsync(classId);
        if (classEntity == null || classEntity.AcademicYearId != id)
        {
            return new AcademicYearActionResult { NotFound = true };
        }

        var hasDuplicate = await _academicYearRepository.ClassNameExistsAsync(id, classEntity.ClassName, classId);
        if (hasDuplicate)
        {
            return new AcademicYearActionResult
            {
                ErrorMessage = $"Không thể kích hoạt lại lớp học này vì lớp '{classEntity.ClassName}' đang hoạt động trong năm học này."
            };
        }

        classEntity.IsDeleted = false;
        await _academicYearRepository.SaveChangesAsync();

        return new AcademicYearActionResult
        {
            Success = true,
            SuccessMessage = $"Lớp học '{classEntity.ClassName}' đã được kích hoạt lại thành công."
        };
    }

    public async Task<AcademicYearActionResult> AddStudentsToClassAsync(int id, int classId, string? studentEmails, int? adminId)
    {
        var classEntity = await _academicYearRepository.GetClassByIdAsync(classId, includeEnrollments: true);
        if (classEntity == null || classEntity.AcademicYearId != id)
        {
            return new AcademicYearActionResult { NotFound = true };
        }

        var parsedEmails = ParseEmails(studentEmails, preserveOrder: true);
        if (parsedEmails.Count == 0)
        {
            return new AcademicYearActionResult { ErrorMessage = "Vui lòng nhập ít nhất một email học sinh." };
        }

        var distinctEmails = parsedEmails
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var students = await _academicYearRepository.GetUsersByEmailsAsync(distinctEmails);
        var existingEnrollments = classEntity.Enrollments
            .Select(e => e.Student?.Email)
            .Where(email => !string.IsNullOrWhiteSpace(email))
            .ToList();

        var seenInputEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var emailsToAdd = new List<User>();
        var errors = new List<string>();

        foreach (var email in parsedEmails)
        {
            if (!seenInputEmails.Add(email))
            {
                errors.Add($"Email '{email}' bị trùng trong danh sách nhập vào, dòng sau đã bị bỏ qua.");
                continue;
            }

            var student = students.FirstOrDefault(s => string.Equals(s.Email, email, StringComparison.OrdinalIgnoreCase));
            if (student == null)
            {
                errors.Add($"Email '{email}' không tồn tại trên hệ thống.");
                continue;
            }

            if (student.Role?.Name != "Student")
            {
                errors.Add($"Tài khoản '{email}' không phải là học sinh.");
                continue;
            }

            if (existingEnrollments.Any(existingEmail => string.Equals(existingEmail, email, StringComparison.OrdinalIgnoreCase)))
            {
                errors.Add($"Học sinh '{email}' đã có sẵn trong lớp này.");
                continue;
            }

            var otherClassEnrollment = await _academicYearRepository.GetOtherClassEnrollmentsByEmailsAsync(id, classId, new[] { email });
            if (otherClassEnrollment.Count > 0)
            {
                var otherClassName = otherClassEnrollment[0].Class?.ClassName ?? "lớp khác";
                errors.Add($"Học sinh '{email}' đang ở {otherClassName}, không thể thêm vào lớp này.");
                continue;
            }

            emailsToAdd.Add(student);
            existingEnrollments.Add(student.Email);
        }

        if (emailsToAdd.Count > 0)
        {
            var newEnrollments = emailsToAdd
                .Select(s => new ClassEnrollment
                {
                    ClassId = classId,
                    StudentId = s.Id
                })
                .ToList();

            await _academicYearRepository.AddClassEnrollmentsAsync(newEnrollments);
            await _academicYearRepository.SaveChangesAsync();
        }

        if (emailsToAdd.Count == 0)
        {
            return new AcademicYearActionResult
            {
                ErrorMessage = errors.Count > 0
                    ? string.Join(" ", errors)
                    : "Không có học sinh hợp lệ để thêm vào lớp."
            };
        }

        return new AcademicYearActionResult
        {
            Success = errors.Count == 0,
            SuccessMessage = errors.Count == 0
                ? $"Đã thêm thành công {emailsToAdd.Count} học sinh vào lớp '{classEntity.ClassName}'."
                : $"Đã thêm thành công {emailsToAdd.Count} học sinh vào lớp '{classEntity.ClassName}', nhưng một số email bị lỗi: {string.Join(" ", errors)}",
            ErrorMessage = errors.Count > 0
                ? $"Đã thêm thành công {emailsToAdd.Count} học sinh vào lớp '{classEntity.ClassName}', nhưng một số email bị lỗi: {string.Join(" ", errors)}"
                : null
        };
    }

    public async Task<AcademicYearActionResult> RemoveStudentFromClassAsync(int id, int classId, string studentEmail, int? adminId)
    {
        var classEntity = await _academicYearRepository.GetClassByIdAsync(classId);
        if (classEntity == null || classEntity.AcademicYearId != id)
        {
            return new AcademicYearActionResult { NotFound = true };
        }

        var student = await _academicYearRepository.GetUserByEmailAsync(studentEmail);
        if (student == null)
        {
            return new AcademicYearActionResult { ErrorMessage = $"Không tìm thấy học sinh '{studentEmail}' trên hệ thống." };
        }

        var enrollment = await _academicYearRepository.GetEnrollmentByClassAndStudentAsync(classId, student.Id);
        if (enrollment == null)
        {
            return new AcademicYearActionResult { ErrorMessage = $"Học sinh '{studentEmail}' không nằm trong lớp này." };
        }

        await _academicYearRepository.RemoveClassEnrollmentAsync(enrollment);
        await _academicYearRepository.SaveChangesAsync();

        return new AcademicYearActionResult
        {
            Success = true,
            SuccessMessage = $"Đã xóa học sinh '{studentEmail}' khỏi lớp '{classEntity.ClassName}' thành công."
        };
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
