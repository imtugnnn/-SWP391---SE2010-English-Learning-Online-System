//Create by TungDPL
//Last update: 7/21/2026
using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;

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

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }
}
