using System.Text.Json;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

// Service xử lý nghiệp vụ chi tiết bài học và kết quả làm bài
public class StudentLessonDetailService : IStudentLessonDetailService
{
    private readonly IStudentLessonDetailRepository _repo;

    public StudentLessonDetailService(IStudentLessonDetailRepository repo)
    {
        _repo = repo;
    }

    // Lấy thông tin chi tiết bài học và tiến độ học tập của học sinh
    public async Task<LessonDetailViewModel?> GetLessonDetailAsync(
        int studentId,
        int lessonId,
        int? assignmentId = null)
    {
        var lesson = await _repo.GetLessonWithContentAsync(studentId, lessonId, assignmentId);
        if (lesson == null) return null;

        var progress = await _repo.GetBestProgressAsync(studentId, lessonId);
        var attemptCount = await _repo.GetAttemptCountAsync(studentId, lessonId);
        var gameProgresses = await _repo.GetGameProgressesAsync(studentId, lessonId);

        var doneGameIds = gameProgresses
            .Select(gp => gp.GameId)
            .ToHashSet();

        return new LessonDetailViewModel
        {
            AssignmentId = assignmentId,
            LessonId = lesson.LessonId,
            Title = lesson.Title,
            Topic = lesson.Topic ?? "",
            CourseName = lesson.Course?.CourseName ?? "",
            XPReward = lesson.XPReward,
            EstimatedMinutes = lesson.EstimatedMinutes,
            CompletionStatus = progress?.CompletionStatus ?? "NOT_STARTED",
            BestScore = progress?.QuizScore ?? 0,
            AttemptCount = attemptCount,

            Vocabularies = lesson.Vocabularies?.Select(v => new VocabItem
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Meaning = v.Meaning,
                ImageUrl = v.ImageUrl ?? ""
            }).ToList() ?? new(),

            Quizzes = lesson.Quizzes?.Select(q => new QuizItem
            {
                QuizId = q.QuizId,
                Question = q.Question,
                QuizType = q.QuizType ?? "",
                OptionsJson = q.Options ?? "[]",
                CorrectAnswer = q.CorrectAnswer
            }).ToList() ?? new(),

            MiniGames = lesson.MiniGames?.Select(g => new MiniGameItem
            {
                GameId = g.GameId,
                Title = g.Title,
                GameType = g.GameType ?? "",
                XPReward = g.XPReward,
                IsDone = doneGameIds.Contains(g.GameId)
            }).ToList() ?? new()
        };
    }

    // Chấm điểm quiz và lưu kết quả làm bài của học sinh
    public async Task<(bool ok, string message)> SubmitQuizAsync(
        int studentId,
        int lessonId,
        Dictionary<int, string> answers)
    {
        var lesson = await _repo.GetLessonWithContentAsync(studentId, lessonId);

        if (lesson == null)
            return (false, "Bài học không tồn tại.");

        var quizzes = lesson.Quizzes?.ToList() ?? new();

        if (!quizzes.Any())
            return (false, "Bài học không có câu hỏi.");

        // Tính số câu trả lời đúng
        int correct = quizzes.Count(q =>
            answers.TryGetValue(q.QuizId, out var ans) &&
            ans.Trim().Equals(
                q.CorrectAnswer.Trim(),
                StringComparison.OrdinalIgnoreCase));

        // Tính điểm và XP thưởng
        int score = (int)((double)correct / quizzes.Count * 100);

        int xpEarned = score >= 80
            ? lesson.XPReward
            : score >= 50
                ? lesson.XPReward / 2
                : 0;

        // Lưu kết quả làm bài
        await _repo.SaveProgressAsync(
            studentId,
            lessonId,
            score,
            JsonSerializer.Serialize(answers),
            xpEarned);

        var msg = score >= 80
            ? $"Xuất sắc! {correct}/{quizzes.Count} đúng. +{xpEarned} XP 🎉"
            : score >= 50
                ? $"Tốt lắm! {correct}/{quizzes.Count} đúng. +{xpEarned} XP"
                : $"Cố gắng hơn nhé! {correct}/{quizzes.Count} đúng.";

        return (true, msg);
    }
}
