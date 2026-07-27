//Create by TungDPL
//Last update: 7/28/2026
using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using EnglishLearningOnlineSystem.Helpers.Admin.Users;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly IRoleService _roleService;
    private readonly AppDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AdminController(IUserService userService, IRoleService roleService, AppDbContext context, IAuditLogService auditLogService)
    {
        _userService = userService;
        _roleService = roleService;
        _context = context;
        _auditLogService = auditLogService;
    }

    public async Task<IActionResult> Dashboard()
    {
        var notificationService = HttpContext.RequestServices.GetRequiredService<ISystemNotificationService>();
        await notificationService.RefreshDueScheduledAsync();
        // Lấy năm học đang hoạt động trước
        var activeAcademicYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.IsActive);

        // 1. Stats grid counts (dữ liệu thực từ database)
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        // Lọc người dùng đang hoạt động có đăng nhập (LastLoginAt) trong tháng này
        var totalUsersAll = await _context.Users.CountAsync();
        var totalUsersActive = await _context.Users.CountAsync(u => u.IsActive && u.LastLoginAt >= startOfMonth);
        var studentCountActive = await _context.Users.CountAsync(u => u.RoleId == 1 && u.IsActive && u.LastLoginAt >= startOfMonth);
        var teacherCountActive = await _context.Users.CountAsync(u => u.RoleId == 3 && u.IsActive && u.LastLoginAt >= startOfMonth);
        
        // Lọc lớp học đang hoạt động dựa vào active academic year
        var activeClassesCount = 0;
        if (activeAcademicYear != null)
        {
            activeClassesCount = await _context.Classes!
                .CountAsync(c => !c.IsDeleted && c.AcademicYearId == activeAcademicYear.AcademicYearId);
        }
        else
        {
            activeClassesCount = await _context.Classes!.CountAsync(c => !c.IsDeleted);
        }
        
        var publishedNotifsThisMonth = await _context.SystemNotifications!
            .CountAsync(n => n.Status == "Đã phát hành" && n.CreatedAt >= startOfMonth);
        var scheduledNotifsThisMonth = await _context.SystemNotifications!
            .CountAsync(n => n.Status == "Đã lên lịch" && n.CreatedAt >= startOfMonth);

        // 2. Phân bố vai trò người dùng (cho biểu đồ Pie)
        var studentCountAll = await _context.Users.CountAsync(u => u.RoleId == 1);
        var teacherCountAll = await _context.Users.CountAsync(u => u.RoleId == 3);
        var parentCountAll = await _context.Users.CountAsync(u => u.RoleId == 4);
        var contentManagerCountAll = await _context.Users.CountAsync(u => u.RoleId == 5);

        // Gán dữ liệu vào ViewBag cho Top Grid Cards
        ViewBag.TotalUsersAll = totalUsersAll;
        ViewBag.TotalUsersActive = totalUsersActive;
        ViewBag.StudentCountActive = studentCountActive;
        ViewBag.TeacherCountActive = teacherCountActive;
        ViewBag.ActiveClassesCount = activeClassesCount;
        ViewBag.ActiveAcademicYear = activeAcademicYear?.YearLabel ?? "Chưa kích hoạt";
        ViewBag.PublishedNotifsThisMonth = publishedNotifsThisMonth;
        ViewBag.ScheduledNotifsThisMonth = scheduledNotifsThisMonth;

        ViewBag.StudentCountAll = studentCountAll;
        ViewBag.TeacherCountAll = teacherCountAll;
        ViewBag.ParentCountAll = parentCountAll;
        ViewBag.ContentManagerCountAll = contentManagerCountAll;
        ViewBag.TotalUsersForChart = studentCountAll + teacherCountAll + parentCountAll + contentManagerCountAll;


        // Lấy 5 system audit log gần nhất
        var latestAuditLogs = _context.AuditLogs != null
            ? await _context.AuditLogs
                .OrderByDescending(al => al.Timestamp)
                .Take(5)
                .ToListAsync()
            : new List<AuditLog>();
        ViewBag.LatestAuditLogs = latestAuditLogs;

        // Lấy danh sách lớp học theo năm học đang hoạt động
        var activeClassesList = new List<Class>();
        if (activeAcademicYear != null)
        {
            activeClassesList = await _context.Classes!
                .Include(c => c.Course)
                .Include(c => c.Teacher)
                .Include(c => c.Enrollments)
                .Where(c => !c.IsDeleted && c.AcademicYearId == activeAcademicYear.AcademicYearId)
                .OrderBy(c => c.ClassName)
                .ToListAsync();
        }
        ViewBag.ActiveClassesList = activeClassesList;

        return View("AdminDashboard");
    }
    
    public async Task<IActionResult> UserManagement()
    {
        var result = await _userService.GetUserManagementDataAsync();
        var roles = await _roleService.GetAllAsync();

        var activeYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.IsActive);

        ViewBag.Roles = roles;
        ViewBag.ActiveAcademicYearId = activeYear?.AcademicYearId;
        var vm = result.Succeeded ? result.Data : new UserManagementViewModel();
        return View("~/Views/Admin/UserManagement/Index.cshtml", vm);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] UserCreateViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _userService.CreateAsync(vm);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi tạo người dùng." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(adminId.Value, $"Tạo người dùng mới: {vm.Username} ({vm.Email}) với vai trò ID {vm.RoleId}");
        }

        return Json(new { success = true });
    }

    // BR-17: Only Administrators can assign or change user roles. (Satisfied since access to these admin endpoints is restricted to Administrators)
    // BR-18: Only Administrators can activate or deactivate user accounts. (Satisfied since access to these admin endpoints is restricted to Administrators)
    [HttpPost]
    public async Task<IActionResult> EditUser([FromBody] UserEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Dữ liệu không hợp lệ." });
        }

        var result = await _userService.UpdateAsync(vm);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi cập nhật người dùng." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(adminId.Value, $"Cập nhật thông tin người dùng: {vm.Username} (ID: {vm.Id})");
        }

        return Json(new { success = true });
    }

    // BR-18: Only Administrators can activate or deactivate user accounts. (Satisfied since access to these admin endpoints is restricted to Administrators)
    [HttpPost]
    public async Task<IActionResult> ToggleUserStatus(int id)
    {
        var userResult = await _userService.GetByIdAsync(id);
        if (!userResult.Succeeded || userResult.Data == null)
        {
            return Json(new { success = false, message = "Không tìm thấy người dùng." });
        }

        var user = userResult.Data;
        if (user.RoleId == 2)
        {
            return Json(new { success = false, message = "Không được phép thay đổi trạng thái tài khoản Admin." });
        }

        var vm = new UserEditViewModel
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            BirthDate = user.BirthDate,
            IsActive = !user.IsActive, // Toggle
            RoleId = user.RoleId,
            Password = null
        };

        var updateResult = await _userService.UpdateAsync(vm);
        if (!updateResult.Succeeded)
        {
            return Json(new { success = false, message = updateResult.ErrorMessage ?? "Lỗi khi cập nhật trạng thái." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            var statusStr = vm.IsActive ? "kích hoạt" : "vô hiệu hóa";
            await _auditLogService.LogActivityAsync(adminId.Value, $"Thay đổi trạng thái của người dùng (ID: {id}) thành {statusStr}");
        }

        return Json(new { success = true, isActive = vm.IsActive });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteAsync(id);
        if (!result.Succeeded)
        {
            return Json(new { success = false, message = result.ErrorMessage ?? "Lỗi khi xóa người dùng." });
        }

        var adminId = GetCurrentUserId();
        if (adminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(adminId.Value, $"Xóa người dùng (ID: {id})");
        }

        return Json(new { success = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportUsersFromExcel(IFormFile importFile)
    {
        if (importFile == null || importFile.Length == 0)
        {
            return Json(new { success = false, message = "Vui lòng chọn tệp Excel." });
        }

        var extension = Path.GetExtension(importFile.FileName).ToLowerInvariant();
        if (extension != ".xlsx")
        {
            return Json(new { success = false, message = "Chỉ hỗ trợ định dạng tệp .xlsx." });
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
            return Json(new { success = false, message = "Không tìm thấy lớp học phù hợp để gán cho học sinh." });
        }

        var existingUsers = await _context.Users
            .AsNoTracking()
            .Select(u => new { u.Username, u.Email })
            .ToListAsync();

        var existingUsernames = new HashSet<string>(existingUsers.Select(u => u.Username), StringComparer.Ordinal);
        var existingEmails = new HashSet<string>(existingUsers.Select(u => u.Email), StringComparer.OrdinalIgnoreCase);
        var fileUsernames = new HashSet<string>(StringComparer.Ordinal);
        var fileEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        List<ExcelUserImportRow> rows;
        await using (var stream = importFile.OpenReadStream())
        {
            rows = UserExcelImportHelper.ReadRows(stream);
        }

        if (rows.Count == 0)
        {
            return Json(new { success = false, message = "File Excel không có dữ liệu hợp lệ." });
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

            var matchedClass = availableClasses.FirstOrDefault(c => string.Equals(c.ClassName.Trim(), className, StringComparison.OrdinalIgnoreCase));
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
            return Json(new
            {
                success = false,
                message = "Không thể import do file có dữ liệu không hợp lệ.",
                errors = validationErrors
            });
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

            var adminId = GetCurrentUserId();
            if (adminId.HasValue)
            {
                await _auditLogService.LogActivityAsync(adminId.Value, $"Import Excel tạo {preparedRows.Count} học sinh mới");
            }

            return Json(new
            {
                success = true,
                importedCount = preparedRows.Count,
                message = $"Đã import thành công {preparedRows.Count} học sinh."
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Json(new
            {
                success = false,
                message = $"Đã xảy ra lỗi khi import Excel: {ex.Message}"
            });
        }
    }

    [HttpGet]
    public IActionResult DownloadUserImportTemplate()
    {
        var templateBytes = UserExcelImportHelper.CreateTemplate();
        const string fileName = "user-import-template.xlsx";
        return File(
            templateBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
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

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }
}
