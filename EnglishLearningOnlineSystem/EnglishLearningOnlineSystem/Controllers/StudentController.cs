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
    private readonly IQuizAttemptService _quizService;
    private readonly IFlashcardService _flashcardService;
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
        IQuizAttemptService quizService,
        IFlashcardService flashcardService,
        IWebHostEnvironment env)
        : base(db)
    {
        _dashboardService = dashboardService;
        _profileService = profileService;
        _courseService = courseService;
        _lessonService = lessonService;
        _lessonDetailService = lessonDetailService;
        _vocabularyService = vocabularyService;
        _quizService = quizService;
        _flashcardService = flashcardService;
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

        // Tự động tạo StudentProfile nếu chưa có (user mới đăng ký)
        if (vm == null)
        {
            await _dashboardService.EnsureStudentProfileAsync(userId.Value);
            vm = await _dashboardService.GetDashboardAsync(userId.Value);
        }

        if (vm == null) return RedirectToAction("Login", "Auth");

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

    // Quiz Flows

    [HttpGet("/student/lesson/{lessonId}/quiz")]
    public async Task<IActionResult> TakeQuiz(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetQuizForLessonAsync(lessonId, userId.Value);
        if (vm == null) return NotFound("Lesson or quizzes not found.");

        return View(vm);
    }

    [HttpPost("/student/lesson/{lessonId}/quiz/submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitQuizAttempt(int lessonId, QuizSubmitViewModel model)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        if (lessonId != model.LessonId) return BadRequest("Lesson ID mismatch.");

        var result = await _quizService.SubmitQuizAsync(userId.Value, model);
        if (result == null) return BadRequest("Error submitting quiz.");

        return RedirectToAction(nameof(QuizResult), new { attemptId = result.AttemptId });
    }

    [HttpGet("/student/quiz/result/{attemptId}")]
    public async Task<IActionResult> QuizResult(int attemptId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetAttemptResultAsync(attemptId, userId.Value);
        if (vm == null) return NotFound();

        return View(vm);
    }

    [HttpGet("/student/history")]
    public async Task<IActionResult> History(int? lessonId, string? from, string? to, string sort = "date")
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetStudentHistoryAsync(userId.Value, lessonId, from, to, sort);
        return View(vm);
    }

    [HttpGet("/student/quiz/review/{attemptId}")]
    public async Task<IActionResult> ReviewIncorrect(int attemptId, bool showAll = false)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _quizService.GetIncorrectAnswersAsync(attemptId, userId.Value, showAll);
        if (vm == null) return NotFound();

        return View(vm);
    }

    // Flashcard Flows 

    [HttpGet("/student/lesson/{lessonId}/flashcards")]
    public async Task<IActionResult> Flashcards(int lessonId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _flashcardService.StartSessionAsync(lessonId, userId.Value);
        if (vm == null) return NotFound("Vocabulary not found for this lesson.");

        return View(vm);
    }

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

    // Hiển thị trang từ vựng tổng hợp (tất cả từ vựng học sinh đã học)
    [HttpGet("/student/vocabulary")]
    public async Task<IActionResult> VocabularyHub()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _vocabularyService.GetAllVocabularyAsync(userId.Value);
        return View(vm);
    }


    // Hiển thị chi tiết khóa học và danh sách bài học
    [HttpGet("/student/courses/{courseId:int}")]
    public async Task<IActionResult> CourseDetail(int courseId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return RedirectToAction("Login", "Auth");

        var vm = await _courseService.GetCourseDetailAsync(userId.Value, courseId);
        if (vm == null) return NotFound();

        return View(vm);
    }

}