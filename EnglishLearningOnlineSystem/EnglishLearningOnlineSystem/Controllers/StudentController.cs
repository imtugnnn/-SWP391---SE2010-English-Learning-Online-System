using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers;

/// <summary>
/// Controller xử lý các chức năng dành cho học sinh như
/// Dashboard, Onboarding và quản lý hồ sơ cá nhân.
/// </summary>
public class StudentController : BaseStudentController
{
    private readonly IStudentDashboardService _dashboardService;
    private readonly IStudentProfileService _profileService;
    private readonly IStudentCourseService _courseService;
    private readonly IWebHostEnvironment _env;

    // Khởi tạo các service và truyền DbContext cho BaseStudentController
    public StudentController(
        AppDbContext db,
        IStudentDashboardService dashboardService,
        IStudentProfileService profileService,
        IStudentCourseService courseService,
        IWebHostEnvironment env)
        : base(db)
    {
        _dashboardService = dashboardService;
        _profileService = profileService;
        _courseService = courseService;
        _env = env;
    }

    // Lấy UserId của người dùng hiện tại từ Session
    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }

    // Hiển thị trang Dashboard của học sinh
    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _dashboardService.GetDashboardAsync(userId.Value);
        if (vm == null)
            return Content($"[Debug] Login OK. UserId = {userId}. StudentProfile chưa có trong DB.");

        // Chuyển sang trang onboarding nếu đăng nhập lần đầu
        if (vm.IsFirstLogin)
            return RedirectToAction(nameof(Onboarding));

        return View(vm);
    }

    // Hiển thị trang onboarding
    [HttpGet("/student/onboarding")]
    public IActionResult Onboarding() => View();

    // Hoàn tất onboarding
    [HttpPost("/student/onboarding/complete")]
    [ValidateAntiForgeryToken]
    public IActionResult OnboardingComplete() => RedirectToAction(nameof(Dashboard));

    // Hiển thị hồ sơ học sinh
    [HttpGet("/student/profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _profileService.GetProfileAsync(userId.Value);
        if (vm == null) return RedirectToAction(nameof(Dashboard));

        return View(vm);
    }

    // Cập nhật thông tin hồ sơ học sinh
    [HttpPost("/student/profile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(StudentProfileViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        // Kiểm tra nickname hợp lệ
        if (string.IsNullOrWhiteSpace(model.NewNickname))
        {
            model.ErrorMessage = "Nickname cannot be empty.";

            var fresh = await _profileService.GetProfileAsync(userId.Value);
            if (fresh != null)
            {
                model.AvatarUrl = fresh.AvatarUrl;
                model.Level = fresh.Level;
                model.XP = fresh.XP;
                model.CurrentStreakDays = fresh.CurrentStreakDays;
                model.Username = fresh.Username;
                model.Email = fresh.Email;
                model.BirthDate = fresh.BirthDate;
            }

            return View(model);
        }

        var ok = await _profileService.UpdateProfileAsync(
            userId.Value,
            model.NewNickname,
            model.AvatarFile,
            _env);

        var vm = await _profileService.GetProfileAsync(userId.Value);
        if (vm == null) return RedirectToAction(nameof(Dashboard));

        // Hiển thị kết quả cập nhật
        vm.SuccessMessage = ok
            ? "Profile updated successfully!"
            : "Update failed. Please try again.";

        if (!ok)
            vm.ErrorMessage = vm.SuccessMessage;

        return View(vm);
    }

    // ── Courses ───────────────────────────────────────────────────────────────

    [HttpGet("/student/courses")]
    public async Task<IActionResult> Courses(string keyword = "", string grade = "")
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _courseService.GetCourseListAsync(userId.Value, keyword, grade);
        return View(vm);
    }

}