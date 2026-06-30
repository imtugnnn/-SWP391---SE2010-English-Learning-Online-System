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

    public AdminController(IUserService userService, IRoleService roleService, AppDbContext context)
    {
        _userService = userService;
        _roleService = roleService;
        _context = context;
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
        var totalUsers = await _context.Users.CountAsync(u => u.IsActive && u.LastLoginAt >= startOfMonth);
        var studentCount = await _context.Users.CountAsync(u => u.RoleId == 1 && u.IsActive && u.LastLoginAt >= startOfMonth);
        var teacherCount = await _context.Users.CountAsync(u => u.RoleId == 3 && u.IsActive && u.LastLoginAt >= startOfMonth);
        
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

        // 3. Tổng quan thông báo (cho biểu đồ Pie)
        var publishedNotifsAll = await _context.SystemNotifications!.CountAsync(n => n.Status == "Đã phát hành");
        var scheduledNotifsAll = await _context.SystemNotifications!.CountAsync(n => n.Status == "Đã lên lịch");
        var draftNotifsAll = await _context.SystemNotifications!.CountAsync(n => n.Status == "Bản nháp");
        var cancelledNotifsAll = await _context.SystemNotifications!.CountAsync(n => n.Status == "Đã hủy");

        // Gán dữ liệu vào ViewBag
        ViewBag.TotalUsers = totalUsers;
        ViewBag.StudentCount = studentCount;
        ViewBag.TeacherCount = teacherCount;
        ViewBag.ActiveClassesCount = activeClassesCount;
        ViewBag.ActiveAcademicYear = activeAcademicYear?.YearLabel ?? "Chưa kích hoạt";
        ViewBag.PublishedNotifsThisMonth = publishedNotifsThisMonth;
        ViewBag.ScheduledNotifsThisMonth = scheduledNotifsThisMonth;

        ViewBag.StudentCountAll = studentCountAll;
        ViewBag.TeacherCountAll = teacherCountAll;
        ViewBag.ParentCountAll = parentCountAll;
        ViewBag.ContentManagerCountAll = contentManagerCountAll;
        ViewBag.TotalUsersForChart = studentCountAll + teacherCountAll + parentCountAll + contentManagerCountAll;

        ViewBag.PublishedNotifsAll = publishedNotifsAll;
        ViewBag.ScheduledNotifsAll = scheduledNotifsAll;
        ViewBag.DraftNotifsAll = draftNotifsAll;
        ViewBag.CancelledNotifsAll = cancelledNotifsAll;
        ViewBag.TotalNotificationsAll = publishedNotifsAll + scheduledNotifsAll + draftNotifsAll + cancelledNotifsAll;

        return View("AdminDashboard");
    }
    
    public async Task<IActionResult> UserManagement()
    {
        var usersResult = await _userService.GetAllAsync();
        var roles = await _roleService.GetAllAsync();

        var activeYear = await _context.AcademicYears!
            .AsNoTracking()
            .FirstOrDefaultAsync(y => y.IsActive);

        ViewBag.Roles = roles;
        ViewBag.ActiveAcademicYearId = activeYear?.AcademicYearId;
        var users = usersResult.Succeeded ? usersResult.Data : new List<User>();
        return View("~/Views/Admin/UserManagement/Index.cshtml", users);
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

        return Json(new { success = true });
    }

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

        return Json(new { success = true });
    }

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

        return Json(new { success = true });
    }
}   
