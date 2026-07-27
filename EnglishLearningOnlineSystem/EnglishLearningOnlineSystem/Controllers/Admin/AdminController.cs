using EnglishLearningOnlineSystem.Helpers.Admin.Users;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AdminController : Controller
{
    private readonly IAcademicYearRepository _academicYearRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IRoleService _roleService;
    private readonly ISystemNotificationService _systemNotificationService;
    private readonly IUserImportService _userImportService;
    private readonly IUserService _userService;

    public AdminController(
        IUserService userService,
        IUserImportService userImportService,
        IRoleService roleService,
        IAuditLogService auditLogService,
        ISystemNotificationService systemNotificationService,
        IAcademicYearRepository academicYearRepository)
    {
        _userService = userService;
        _userImportService = userImportService;
        _roleService = roleService;
        _auditLogService = auditLogService;
        _systemNotificationService = systemNotificationService;
        _academicYearRepository = academicYearRepository;
    }

    public async Task<IActionResult> Dashboard()
    {
        var userManagementResult = await _userService.GetUserManagementDataAsync();
        var stats = userManagementResult.Data?.Stats ?? new UserStatsViewModel();

        var activeAcademicYear = (await _academicYearRepository.GetActiveAcademicYearsAsync()).FirstOrDefault();
        var activeClassesList = activeAcademicYear != null
            ? await _academicYearRepository.GetClassesByAcademicYearIdAsync(activeAcademicYear.AcademicYearId)
            : new List<Class>();

        var notificationsResult = await _systemNotificationService.GetIndexDataAsync();
        var notifications = notificationsResult.Succeeded && notificationsResult.Data != null
            ? notificationsResult.Data.Notifications
            : new List<SystemNotification>();

        var now = DateTime.Now;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        ViewBag.TotalUsersAll = stats.TotalUsers;
        ViewBag.TotalUsersActive = stats.ActiveThisMonth;
        ViewBag.StudentCountActive = stats.StudentsThisMonth;
        ViewBag.TeacherCountActive = stats.TeachersThisMonth;
        ViewBag.ActiveClassesCount = activeClassesList.Count;
        ViewBag.ActiveAcademicYear = activeAcademicYear?.YearLabel ?? "Chưa kích hoạt";
        ViewBag.PublishedNotifsThisMonth = notifications.Count(n =>
            n.Status == "Đã phát hành" &&
            (n.PublishTime ?? n.CreatedAt) >= startOfMonth);
        ViewBag.ScheduledNotifsThisMonth = notifications.Count(n =>
            n.Status == "Đã lên lịch" &&
            (n.PublishTime ?? n.CreatedAt) >= startOfMonth);

        ViewBag.StudentCountAll = stats.StudentCount;
        ViewBag.TeacherCountAll = stats.TeacherCount;
        ViewBag.ParentCountAll = stats.ParentCount;
        ViewBag.ContentManagerCountAll = stats.ContentManagerCount;
        ViewBag.TotalUsersForChart = stats.StudentCount + stats.TeacherCount + stats.ParentCount + stats.ContentManagerCount;
        ViewBag.LatestAuditLogs = await _auditLogService.GetLatestAsync(5);
        ViewBag.ActiveClassesList = activeClassesList;
        ViewBag.ActiveAcademicYearId = activeAcademicYear?.AcademicYearId;

        return View("AdminDashboard");
    }

    public async Task<IActionResult> UserManagement()
    {
        var result = await _userService.GetUserManagementDataAsync();
        var roles = await _roleService.GetAllAsync();
        var activeYear = (await _academicYearRepository.GetActiveAcademicYearsAsync()).FirstOrDefault();

        ViewBag.Roles = roles;
        ViewBag.ActiveAcademicYearId = activeYear?.AcademicYearId;

        var vm = result.Succeeded && result.Data != null ? result.Data : new UserManagementViewModel();
        return View("~/Views/Admin/UserManagement/Index.cshtml", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportUsersFromExcel(IFormFile importFile)
    {
        var result = await _userImportService.ImportUsersFromExcelAsync(importFile);
        if (result.Succeeded)
        {
            var adminId = GetCurrentUserId();
            if (adminId.HasValue)
            {
                await _auditLogService.LogActivityAsync(adminId.Value, $"Import Excel tạo {result.ImportedCount} học sinh mới");
            }
        }

        return Json(new
        {
            success = result.Succeeded,
            importedCount = result.ImportedCount,
            message = result.Message,
            errors = result.Errors
        });
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

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }
}
