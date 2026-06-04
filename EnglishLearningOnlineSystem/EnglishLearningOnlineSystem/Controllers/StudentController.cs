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
    private readonly IStudentLessonService _lessonService;
    private readonly IStudentLessonDetailService _lessonDetailService;
    private readonly IVocabularyService _vocabularyService;
    private readonly IWebHostEnvironment _env;

    // Khởi tạo các service và truyền DbContext cho BaseStudentController
    public StudentController(
        AppDbContext db,
        IStudentDashboardService dashboardService,
        IStudentProfileService profileService,
        IStudentCourseService courseService,
        IStudentLessonService lessonService,
        IStudentLessonDetailService lessonDetailService,
        IVocabularyService vocabularyService,
        IWebHostEnvironment env)
        : base(db)
    {
        _dashboardService = dashboardService;
        _profileService = profileService;
        _courseService = courseService;
        _lessonService = lessonService;
        _lessonDetailService = lessonDetailService;
        _vocabularyService = vocabularyService;
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

    // Hiển thị danh sách bài học được giao cho học sinh
    [HttpGet("/student/lessons")]
    public async Task<IActionResult> Lessons(string status = "")
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _lessonService.GetAssignedLessonsAsync(userId.Value, status);
        return View(vm);
    }

    // Hiển thị chi tiết bài học và nội dung học tập
    [HttpGet("/student/lesson/{lessonId:int}")]
    public async Task<IActionResult> LessonDetail(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _lessonDetailService.GetLessonDetailAsync(userId.Value, lessonId);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // Nộp bài quiz và lưu kết quả làm bài của học sinh
    [HttpPost("/student/lesson/{lessonId:int}/submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(int lessonId, IFormCollection form)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        // Thu thập các đáp án được gửi từ form
        var answers = new Dictionary<int, string>();

        foreach (var key in form.Keys.Where(k => k.StartsWith("quiz_")))
        {
            if (int.TryParse(key.Replace("quiz_", ""), out var qId))
                answers[qId] = form[key].ToString();
        }

        var (ok, message) = await _lessonDetailService.SubmitQuizAsync(
            userId.Value,
            lessonId,
            answers);

        // Lưu kết quả để hiển thị sau khi redirect
        TempData["QuizResult"] = message;
        TempData["QuizOk"] = ok.ToString();

        return RedirectToAction(nameof(LessonDetail), new { lessonId });
    }

    // Hiển thị danh sách từ vựng của một bài học
    [HttpGet("/student/lesson/{lessonId:int}/vocabulary")]
    public async Task<IActionResult> Vocabulary(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _vocabularyService.GetVocabularyAsync(lessonId);
        if (vm == null) return NotFound();

        return View(vm);
    }

}