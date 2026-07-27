//Create by TungDPL
//Create at 7/28/2026
//Last update: 7/28/2026
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Helpers.Admin.Users;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class UserImportService : IUserImportService
{
    private readonly AppDbContext _context;

    public UserImportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<UserImportServiceResult> ImportUsersFromExcelAsync(IFormFile importFile)
    {
        if (importFile == null || importFile.Length == 0)
        {
            return UserImportServiceResult.Fail("Vui lòng chọn tệp Excel.");
        }

        var extension = Path.GetExtension(importFile.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return UserImportServiceResult.Fail("Chỉ hỗ trợ định dạng tệp .xlsx.");
        }

        var activeYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.IsActive);

        var classQuery = _context.Classes!
            .AsNoTracking()
            .Where(c => !c.IsDeleted);

        if (activeYear != null)
        {
            classQuery = classQuery.Where(c => c.AcademicYearId == activeYear.AcademicYearId);
        }

        var availableClasses = await classQuery.ToListAsync();
        if (availableClasses.Count == 0)
        {
            return UserImportServiceResult.Fail("Không tìm thấy lớp học phù hợp để gán cho học sinh.");
        }

        var existingUsers = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.Username, u.Email })
            .ToListAsync();

        var existingUsernames = new HashSet<string>(
            existingUsers.Where(u => !string.IsNullOrWhiteSpace(u.Username)).Select(u => u.Username!),
            StringComparer.Ordinal);

        var existingEmails = new HashSet<string>(
            existingUsers.Where(u => !string.IsNullOrWhiteSpace(u.Email)).Select(u => u.Email!),
            StringComparer.OrdinalIgnoreCase);

        var fileUsernames = new HashSet<string>(StringComparer.Ordinal);
        var fileEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        List<ExcelUserImportRow> rows;
        await using (var stream = importFile.OpenReadStream())
        {
            rows = UserExcelImportHelper.ReadRows(stream);
        }

        if (rows.Count == 0)
        {
            return UserImportServiceResult.Fail("File Excel không có dữ liệu hợp lệ.");
        }

        var validationErrors = new List<string>();
        var preparedRows = new List<(ExcelUserImportRow Row, Class ClassEntity)>();

        foreach (var row in rows)
        {
            var username = row.Username.Trim();
            var email = row.Email.Trim();
            var className = row.ClassName.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: username không được để trống.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: email không được để trống.");
                continue;
            }

            if (!email.Contains('@'))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: email không hợp lệ.");
                continue;
            }

            if (!row.BirthDate.HasValue)
            {
                validationErrors.Add($"Dòng {row.RowNumber}: ngày sinh không hợp lệ hoặc bị thiếu.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(className))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: lớp không được để trống.");
                continue;
            }

            if (!IsStudentOldEnough(row.BirthDate.Value))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: học sinh phải lớn hơn 6 tuổi.");
                continue;
            }

            var matchedClass = availableClasses.FirstOrDefault(c =>
                string.Equals(c.ClassName?.Trim(), className, StringComparison.OrdinalIgnoreCase));

            if (matchedClass == null)
            {
                validationErrors.Add($"Dòng {row.RowNumber}: không tìm thấy lớp '{className}' trong năm học hiện tại.");
                continue;
            }

            if (existingUsernames.Contains(username) || fileUsernames.Contains(username))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: username '{username}' đã tồn tại.");
                continue;
            }

            if (existingEmails.Contains(email) || fileEmails.Contains(email))
            {
                validationErrors.Add($"Dòng {row.RowNumber}: email '{email}' đã tồn tại.");
                continue;
            }

            fileUsernames.Add(username);
            fileEmails.Add(email);
            preparedRows.Add((row with { Username = username, Email = email, ClassName = className }, matchedClass));
        }

        if (validationErrors.Count > 0)
        {
            return UserImportServiceResult.Fail(
                "Không thể import do file có dữ liệu không hợp lệ.",
                validationErrors);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var createdUsers = new List<User>();
            foreach (var item in preparedRows)
            {
                var birthDate = item.Row.BirthDate.GetValueOrDefault().Date;
                var user = new User
                {
                    Username = item.Row.Username,
                    Email = item.Row.Email,
                    Password = "123456",
                    BirthDate = birthDate,
                    IsActive = true,
                    RoleId = 1
                };

                _context.Users.Add(user);
                createdUsers.Add(user);
            }

            await _context.SaveChangesAsync();

            foreach (var item in preparedRows.Select((value, index) => new { value, index }))
            {
                var user = createdUsers[item.index];

                _context.StudentProfiles!.Add(new StudentProfile
                {
                    StudentId = user.Id,
                    Nickname = user.Username,
                    AvatarUrl = "/images/default-avatar.png",
                    Level = 1,
                    XP = 0,
                    CurrentStreakDays = 0,
                    LastActiveDate = null
                });

                _context.ClassEnrollments!.Add(new ClassEnrollment
                {
                    ClassId = item.value.ClassEntity.ClassId,
                    StudentId = user.Id,
                    EnrolledAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return UserImportServiceResult.Ok(
                preparedRows.Count,
                $"Đã import thành công {preparedRows.Count} học sinh.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return UserImportServiceResult.Fail($"Đã xảy ra lỗi khi import Excel: {ex.Message}");
        }
    }

    private static bool IsStudentOldEnough(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;
        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return age > 6;
    }
}
