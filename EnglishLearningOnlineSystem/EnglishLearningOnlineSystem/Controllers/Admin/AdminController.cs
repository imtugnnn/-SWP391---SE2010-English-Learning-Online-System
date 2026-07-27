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
    private readonly IRoleService _roleService;
    private readonly ISystemNotificationService _systemNotificationService;
    private readonly IUserImportService _userImportService;
    private readonly IUserService _userService;

    public AdminController(
        IUserService userService,
        IUserImportService userImportService,
        IRoleService roleService,
        ISystemNotificationService systemNotificationService,
        IAcademicYearRepository academicYearRepository)
    {
        _userService = userService;
        _userImportService = userImportService;
        _roleService = roleService;
        _systemNotificationService = systemNotificationService;
        _academicYearRepository = academicYearRepository;
    }

    public async Task<IActionResult> Dashboard()
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        //Nếu người dùng khoông phải là admin, không cho đăng nhập
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }
        // B1: Gom số liệu từ service/repository để dựng dashboard admin.
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
        ViewBag.ActiveClassesList = activeClassesList;
        ViewBag.ActiveAcademicYearId = activeAcademicYear?.AcademicYearId;

        return View("AdminDashboard");
    }

    public async Task<IActionResult> UserManagement()
    {
        var userRoleSession = HttpContext.Session.GetString("UserRole");
        //Nếu người dùng khoông phải là admin, không cho đăng nhập
        if (userRoleSession != "2")
        {
            return RedirectToAction("Login", "Auth");
        }
        // B1: Load data cho màn quản lý user trước khi render view.
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
        // B2: Endpoint này nhận file từ form JS fetch() rồi đẩy xuống service import.
        var result = await _userImportService.ImportUsersFromExcelAsync(importFile);

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
        // B1: Tạo file mẫu Excel để admin tải về.
        var templateBytes = UserExcelImportHelper.CreateTemplate();
        const string fileName = "user-import-template.xlsx";
        return File(
            templateBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }
}
