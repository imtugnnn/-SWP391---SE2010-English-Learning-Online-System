using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

/// <summary>
/// Service phụ trách toàn bộ luồng nghiệp vụ của bài kiểm tra (Quiz).
/// Bao gồm việc lấy danh sách câu hỏi, chấm điểm nộp bài (submit),
/// thưởng điểm kinh nghiệm (XP) nếu đạt, và lấy lịch sử/xem lại lỗi sai.
/// </summary>
public class QuizAttemptService : IQuizAttemptService
{
    private readonly IQuizAttemptRepository _quizRepo;
    private readonly IStudentDashboardRepository _dashboardRepo;
    private readonly IAssignmentProgressService _assignmentProgressService;

    public QuizAttemptService(
        IQuizAttemptRepository quizRepo,
        IStudentDashboardRepository dashboardRepo,
        IAssignmentProgressService assignmentProgressService)
    {
        _quizRepo = quizRepo;
        _dashboardRepo = dashboardRepo;
        _assignmentProgressService = assignmentProgressService;
    }

    public async Task<TakeQuizViewModel?> GetQuizForLessonAsync(
        int lessonId,
        int studentId,
        int? assignmentId = null)
    {
        var lesson = await _quizRepo.GetLessonByIdAsync(lessonId);
        if (lesson == null) return null;

        var assignment = await _quizRepo.GetAssignmentForLessonAsync(
            lessonId,
            studentId,
            assignmentId);

        // AssignmentId có trên URL nhưng không thuộc học sinh/bài học hiện tại.
        if (assignmentId.HasValue && assignment == null)
        {
            return null;
        }

        if (assignment != null)
        {
            await _assignmentProgressService.MarkActivityStartedAsync(
                assignment.AssignmentId, studentId, AssignmentActivityType.Quiz);
        }

        var quizzes = await _quizRepo.GetQuizzesByLessonIdAsync(
            lessonId,
            studentId,
            assignment?.AssignmentId);
        
        bool isOverdue = assignment != null && assignment.DueDate.Date < DateTime.Today;

        return new TakeQuizViewModel
        {
            LessonId = lessonId,
            LessonTitle = lesson.Title,
            WeeklyAssignmentId = assignment?.AssignmentId,
            IsOverdue = isOverdue,
            DueDate = assignment?.DueDate,
            Questions = quizzes.Select(q => new QuizQuestionItem
            {
                QuizId = q.QuizId,
                Question = q.Question,
                QuizType = q.QuizType,
                Options = string.IsNullOrEmpty(q.Options) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(q.Options) ?? new List<string>()
            }).ToList()
        };
    }

    public async Task<QuizResultViewModel?> SubmitQuizAsync(int studentId, QuizSubmitViewModel submitData)
    {
        var lesson = await _quizRepo.GetLessonByIdAsync(submitData.LessonId);
        if (lesson == null) return null;

        var assignment = await _quizRepo.GetAssignmentForLessonAsync(
            submitData.LessonId,
            studentId,
            submitData.WeeklyAssignmentId);

        // Không tin cậy AssignmentId gửi từ hidden input: luôn xác thực lại quyền truy cập.
        if (submitData.WeeklyAssignmentId.HasValue && assignment == null)
        {
            return null;
        }

        var quizzes = await _quizRepo.GetQuizzesByLessonIdAsync(
            submitData.LessonId,
            studentId,
            assignment?.AssignmentId);
        if (!quizzes.Any()) return null;

        int totalQuestions = quizzes.Count;
        int correctCount = 0;
        var answersToSave = new List<QuizAttemptAnswer>();

        foreach (var quiz in quizzes)
        {
            submitData.Answers.TryGetValue(quiz.QuizId, out var selectedAns);
            selectedAns ??= string.Empty;

            // So sánh đáp án học sinh chọn với đáp án đúng (không phân biệt hoa/thường)
            bool isCorrect = selectedAns.Trim().Equals(quiz.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);
            if (isCorrect) correctCount++;

            answersToSave.Add(new QuizAttemptAnswer
            {
                QuizId = quiz.QuizId,
                SelectedAnswer = selectedAns,
                IsCorrect = isCorrect
            });
        }

        // Tính điểm theo thang phần trăm (0-100)
        int score = (int)Math.Round((double)correctCount / totalQuestions * 100);
        var startedAt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(submitData.StartedAtTicks);
        int timeSpentSec = (int)(DateTime.UtcNow - startedAt).TotalSeconds;
        if (timeSpentSec < 0) timeSpentSec = 0;

        var existingProgress = await _quizRepo.GetProgressAsync(studentId, submitData.LessonId);
        bool isFirstCompletion = existingProgress == null || existingProgress.CompletionStatus != "Completed";
        bool alreadyAwardedXp = existingProgress != null && existingProgress.XPEarned > 0;
        
        // Theo quy tắc nghiệp vụ BR-14: Học sinh chỉ được thưởng XP 1 lần duy nhất 
        // cho mỗi bài học khi đạt điểm >= 50%. Các lần làm lại (re-attempt) không được cộng thêm.
        bool willAwardXp = !alreadyAwardedXp && score >= 50;
        int xpEarned = willAwardXp ? lesson.XPReward : 0;

        var attempt = new QuizAttempt
        {
            StudentId = studentId,
            LessonId = submitData.LessonId,
            WeeklyAssignmentId = assignment?.AssignmentId,
            TotalQuestions = totalQuestions,
            CorrectCount = correctCount,
            Score = score,
            TimeSpentSec = timeSpentSec,
            StartedAt = startedAt,
            SubmittedAt = DateTime.UtcNow,
            XpAwarded = willAwardXp,
            Answers = answersToSave
        };

        await _quizRepo.CreateAttemptAsync(attempt);

        if (existingProgress == null)
        {
            // Lần nộp bài đầu tiên: Ghi nhận tiến độ mới
            var progress = new Progress
            {
                StudentId = studentId,
                LessonId = submitData.LessonId,
                CompletionStatus = "Completed",
                QuizScore = score,
                XPEarned = xpEarned,
                CompletedAt = DateTime.UtcNow,
                IsBestAttempt = true
            };
            await _quizRepo.CreateProgressAsync(progress);
        }
        else
        {
            // Đã từng làm bài này: Cập nhật lại điểm cao nhất (nếu có) và trạng thái
            if (score > existingProgress.QuizScore)
            {
                existingProgress.QuizScore = score;
            }
            if (willAwardXp)
            {
                existingProgress.XPEarned += xpEarned;
            }
            if (existingProgress.CompletionStatus != "Completed")
            {
                existingProgress.CompletionStatus = "Completed";
                existingProgress.CompletedAt = DateTime.UtcNow;
            }
            await _quizRepo.UpdateProgressAsync(existingProgress);
        }

        if (assignment != null)
        {
            // Business process: một lần submit hợp lệ hoàn tất activity Quiz; điểm cao nhất được giữ riêng.
            await _assignmentProgressService.MarkActivityCompletedAsync(
                assignment.AssignmentId,
                studentId,
                AssignmentActivityType.Quiz,
                score: score);
        }

        return await GetAttemptResultAsync(attempt.AttemptId, studentId);
    }

    public async Task<QuizResultViewModel?> GetAttemptResultAsync(int attemptId, int studentId)
    {
        var attempt = await _quizRepo.GetAttemptByIdAsync(attemptId, studentId);
        if (attempt == null) return null;

        var existingProgress = await _quizRepo.GetProgressAsync(studentId, attempt.LessonId);
        bool alreadyAwardedXp = existingProgress != null && existingProgress.XPEarned > 0 && !attempt.XpAwarded;

        return new QuizResultViewModel
        {
            AttemptId = attempt.AttemptId,
            LessonId = attempt.LessonId,
            LessonTitle = attempt.Lesson?.Title ?? "",
            WeeklyAssignmentId = attempt.WeeklyAssignmentId,
            Score = attempt.Score,
            CorrectCount = attempt.CorrectCount,
            TotalQuestions = attempt.TotalQuestions,
            XpEarned = attempt.XpAwarded ? (attempt.Lesson?.XPReward ?? 0) : 0,
            AlreadyAwardedXp = alreadyAwardedXp,
            TimeSpentSec = attempt.TimeSpentSec,
            IsCompletedLate = attempt.WeeklyAssignmentId.HasValue &&
                (await _assignmentProgressService.GetSnapshotAsync(
                    attempt.WeeklyAssignmentId.Value, studentId))?.IsCompletedLate == true,
            AnswerResults = attempt.Answers.Select(a => new QuizAnswerResultItem
            {
                QuizId = a.QuizId,
                Question = a.Quiz.Question,
                QuizType = a.Quiz.QuizType,
                SelectedAnswer = a.SelectedAnswer,
                CorrectAnswer = a.Quiz.CorrectAnswer,
                IsCorrect = a.IsCorrect,
                AllOptions = string.IsNullOrEmpty(a.Quiz.Options) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(a.Quiz.Options) ?? new List<string>()
            }).ToList()
        };
    }

    public async Task<AttemptHistoryViewModel> GetStudentHistoryAsync(int studentId, int? lessonId, string? from, string? to, string sort)
    {
        DateTime? fromDate = null;
        if (DateTime.TryParse(from, out var f)) fromDate = f;
        
        DateTime? toDate = null;
        if (DateTime.TryParse(to, out var t)) toDate = t;

        var attempts = await _quizRepo.GetAttemptsByStudentAsync(studentId, lessonId, fromDate, toDate, sort);
        var dashboard = await _dashboardRepo.GetProfileByUserIdAsync(studentId);
        var lessons = await _quizRepo.GetLessonsWithAttemptsAsync(studentId);

        return new AttemptHistoryViewModel
        {
            Attempts = attempts.Select(a => new AttemptSummaryItem
            {
                AttemptId = a.AttemptId,
                LessonId = a.LessonId,
                LessonTitle = a.Lesson?.Title ?? "",
                Score = a.Score,
                CorrectCount = a.CorrectCount,
                TotalQuestions = a.TotalQuestions,
                TimeSpentSec = a.TimeSpentSec,
                SubmittedAt = a.SubmittedAt,
                XpAwarded = a.XpAwarded
            }).ToList(),
            FilterLessonId = lessonId,
            FilterFrom = from,
            FilterTo = to,
            SortBy = sort,
            AvailableLessons = lessons.Select(l => new LessonFilterItem
            {
                LessonId = l.LessonId,
                Title = l.Title
            }).ToList(),
            Nickname = dashboard?.Nickname ?? dashboard?.User?.Username ?? "Student",
            AvatarUrl = dashboard?.AvatarUrl ?? "/images/default-avatar.png",
            Level = dashboard?.Level ?? 1,
            XP = dashboard?.XP ?? 0,
            CurrentStreakDays = dashboard?.CurrentStreakDays ?? 0
        };
    }

    public async Task<ReviewIncorrectViewModel?> GetIncorrectAnswersAsync(int attemptId, int studentId, bool showAll = false)
    {
        var attempt = await _quizRepo.GetAttemptByIdAsync(attemptId, studentId);
        if (attempt == null) return null;

        var dashboard = await _dashboardRepo.GetProfileByUserIdAsync(studentId);

        var items = attempt.Answers
            .Where(a => showAll || !a.IsCorrect)
            .Select(a => new ReviewAnswerItem
            {
                QuizId = a.QuizId,
                Question = a.Quiz.Question,
                QuizType = a.Quiz.QuizType,
                YourAnswer = a.SelectedAnswer,
                CorrectAnswer = a.Quiz.CorrectAnswer,
                IsCorrect = a.IsCorrect,
                AllOptions = string.IsNullOrEmpty(a.Quiz.Options) ? new List<string>() : System.Text.Json.JsonSerializer.Deserialize<List<string>>(a.Quiz.Options) ?? new List<string>()
            }).ToList();

        return new ReviewIncorrectViewModel
        {
            AttemptId = attempt.AttemptId,
            LessonTitle = attempt.Lesson?.Title ?? "",
            Score = attempt.Score,
            CorrectCount = attempt.CorrectCount,
            TotalQuestions = attempt.TotalQuestions,
            SubmittedAt = attempt.SubmittedAt,
            ShowAll = showAll,
            Items = items,
            Nickname = dashboard?.Nickname ?? dashboard?.User?.Username ?? "Student",
            AvatarUrl = dashboard?.AvatarUrl ?? "/images/default-avatar.png",
            Level = dashboard?.Level ?? 1
        };
    }
}
