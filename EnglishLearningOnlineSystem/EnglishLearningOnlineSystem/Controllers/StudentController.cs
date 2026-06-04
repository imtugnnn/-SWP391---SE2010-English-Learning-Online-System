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
    private readonly IQuizAttemptService _quizService;
    private readonly IFlashcardService _flashcardService;
    private readonly IStudentProfileService _profileService;
    private readonly IWebHostEnvironment _env;

    public StudentController(
        AppDbContext db,
        IStudentDashboardService dashboardService,
        IQuizAttemptService quizService,
        IFlashcardService flashcardService,
        IStudentProfileService profileService,
        IWebHostEnvironment env)
        : base(db)
    {
        _dashboardService = dashboardService;
        _quizService = quizService;
        _flashcardService = flashcardService;
        _profileService = profileService;
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

    // ==========================================
    // QUIZ FLOWS (Luồng làm bài kiểm tra)
    // ==========================================

    // Hiển thị giao diện làm bài Quiz cho một bài học cụ thể
    [HttpGet("/student/lesson/{lessonId}/quiz")]
    public async Task<IActionResult> TakeQuiz(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetQuizForLessonAsync(lessonId, userId.Value);
        if (vm == null) return NotFound("Lesson or quizzes not found.");

        return View(vm);
    }

    // Xử lý logic khi học sinh bấm "Nộp bài" (Submit)
    // - Tính điểm, kiểm tra hạn chót, thưởng XP, và lưu vào CSDL
    [HttpPost("/student/lesson/{lessonId}/quiz/submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuiz(int lessonId, QuizSubmitViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        if (lessonId != model.LessonId) return BadRequest("Lesson ID mismatch.");

        var result = await _quizService.SubmitQuizAsync(userId.Value, model);
        if (result == null) return BadRequest("Error submitting quiz.");

        return RedirectToAction(nameof(QuizResult), new { attemptId = result.AttemptId });
    }

    // Hiển thị màn hình Kết quả (Điểm số, XP nhận được) ngay sau khi nộp bài
    [HttpGet("/student/quiz/result/{attemptId}")]
    public async Task<IActionResult> QuizResult(int attemptId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetAttemptResultAsync(attemptId, userId.Value);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // Hiển thị Lịch sử các lần làm bài của học sinh (hỗ trợ lọc theo ngày, bài học)
    [HttpGet("/student/history")]
    public async Task<IActionResult> History(int? lessonId, string? from, string? to, string sort = "date")
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetStudentHistoryAsync(userId.Value, lessonId, from, to, sort);
        return View(vm);
    }

    // Hiển thị màn hình "Xem lại lỗi sai" để học sinh đối chiếu đáp án chọn với đáp án đúng
    [HttpGet("/student/quiz/review/{attemptId}")]
    public async Task<IActionResult> ReviewIncorrect(int attemptId, bool showAll = false)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetIncorrectAnswersAsync(attemptId, userId.Value, showAll);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // ==========================================
    // FLASHCARD FLOWS (Luồng ôn tập thẻ từ vựng)
    // ==========================================

    // Hiển thị giao diện lật thẻ (Flashcards) cho một bài học
    [HttpGet("/student/lesson/{lessonId}/flashcards")]
    public async Task<IActionResult> Flashcards(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _flashcardService.StartSessionAsync(lessonId, userId.Value);
        if (vm == null) return NotFound("Vocabulary not found for this lesson.");

        return View(vm);
    }

    // Xử lý nộp kết quả tự đánh giá Flashcard ("Đã thuộc" / "Chưa thuộc")
    [HttpPost("/student/lesson/{lessonId}/flashcards/submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitFlashcards(int lessonId, FlashcardCompleteViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        if (lessonId != model.LessonId) return BadRequest("Lesson ID mismatch.");

        await _flashcardService.CompleteSessionAsync(userId.Value, model);
        
        return Redirect($"/student/flashcards/result/{model.SessionId}");
    }

    // Hiển thị bảng tổng kết số lượng thẻ đã học và tỉ lệ nhớ từ vựng
    [HttpGet("/student/flashcards/result/{sessionId}")]
    public async Task<IActionResult> FlashcardResult(int sessionId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _flashcardService.GetSessionResultAsync(sessionId, userId.Value);
        if (vm == null) return NotFound();

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
}