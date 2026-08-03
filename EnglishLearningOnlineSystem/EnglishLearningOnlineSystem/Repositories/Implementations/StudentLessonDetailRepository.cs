using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

// Repository xử lý dữ liệu chi tiết bài học và tiến độ học tập
public class StudentLessonDetailRepository : IStudentLessonDetailRepository
{
    private readonly AppDbContext _db;

    public StudentLessonDetailRepository(AppDbContext db)
    {
        _db = db;
    }

    // Lấy thông tin bài học cùng toàn bộ nội dung liên quan
    public async Task<Lesson?> GetLessonWithContentAsync(
        int studentId,
        int lessonId,
        int? assignmentId = null)
    {
        var lesson = await _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .Include(l => l.Vocabularies)
            .Include(l => l.Quizzes)
            .Include(l => l.MiniGames)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.IsPublished);

        if (lesson == null)
        {
            return null;
        }

        var assignment = await _db.WeeklyAssignments!
            .AsNoTracking()
            .Include(x => x.Vocabularies)
            .Include(x => x.Quizzes)
            .Include(x => x.MiniGames)
            .Where(x =>
                x.LessonId == lessonId &&
                x.IsVisible &&
                (!assignmentId.HasValue || x.AssignmentId == assignmentId.Value) &&
                x.ClassId.HasValue &&
                _db.ClassEnrollments!.Any(e =>
                    e.ClassId == x.ClassId.Value && e.StudentId == studentId))
            .OrderByDescending(x => x.WeekStartDate)
            .FirstOrDefaultAsync();

        if (assignment == null && assignmentId.HasValue)
        {
            return null;
        }

        if (assignment == null)
        {
            return lesson;
        }

        var vocabularyIds = assignment.Vocabularies.Select(x => x.VocabularyId).ToHashSet();
        var quizIds = assignment.Quizzes.Select(x => x.QuizId).ToHashSet();
        var gameIds = assignment.MiniGames.Select(x => x.GameId).ToHashSet();

        lesson.Vocabularies = assignment.IncludeVocabulary
            ? lesson.Vocabularies.Where(x => vocabularyIds.Contains(x.VocabularyId)).ToList()
            : new List<Vocabulary>();
        lesson.Quizzes = assignment.IncludeQuiz
            ? lesson.Quizzes.Where(x => quizIds.Contains(x.QuizId)).ToList()
            : new List<Quiz>();
        lesson.MiniGames = assignment.IncludeMiniGame
            ? lesson.MiniGames.Where(x => gameIds.Contains(x.GameId)).ToList()
            : new List<MiniGame>();

        return lesson;
    }

    // Lấy kết quả làm bài tốt nhất của học sinh
    public async Task<Progress?> GetBestProgressAsync(int studentId, int lessonId)
    {
        return await _db.Progresses!
            .Where(p => p.StudentId == studentId
                     && p.LessonId == lessonId
                     && p.IsBestAttempt)
            .FirstOrDefaultAsync();
    }

    // Đếm số lần học sinh làm bài quiz
    public async Task<int> GetAttemptCountAsync(int studentId, int lessonId)
    {
        return await _db.Progresses!
            .Where(p => p.StudentId == studentId && p.LessonId == lessonId)
            .CountAsync();
    }

    // Lấy tiến độ hoàn thành các mini game trong bài học
    public async Task<List<StudentGameProgress>> GetGameProgressesAsync(int studentId, int lessonId)
    {
        var gameIds = await _db.MiniGames!
            .Where(g => g.LessonId == lessonId)
            .Select(g => g.GameId)
            .ToListAsync();

        return await _db.StudentGameProgresses!
            .Where(gp => gp.StudentId == studentId && gameIds.Contains(gp.GameId))
            .ToListAsync();
    }

    // Lưu kết quả làm bài và cập nhật XP cho học sinh
    public async Task SaveProgressAsync(
        int studentId,
        int lessonId,
        int score,
        string answersJson,
        int xpEarned)
    {
        // Tìm kết quả tốt nhất hiện tại
        var prevBest = await _db.Progresses!
            .Where(p => p.StudentId == studentId
                     && p.LessonId == lessonId
                     && p.IsBestAttempt)
            .FirstOrDefaultAsync();

        var isNewBest = prevBest == null || score > prevBest.QuizScore;

        // Cập nhật trạng thái bản ghi tốt nhất trước đó
        if (prevBest != null && isNewBest)
            prevBest.IsBestAttempt = false;

        var status = score >= 50 ? "Completed" : "In Progress";

        var progress = new Progress
        {
            StudentId = studentId,
            LessonId = lessonId,
            QuizScore = score,
            XPEarned = isNewBest ? xpEarned : 0,
            CompletionStatus = status,
            IsBestAttempt = isNewBest,
            CompletedAt = DateTime.Now
        };

        _db.Progresses!.Add(progress);

        // Ghi nhận XP nếu đạt kết quả tốt nhất mới
        if (isNewBest && xpEarned > 0)
        {
            _db.XpTransactions!.Add(new XpTransaction
            {
                StudentId = studentId,
                Amount = xpEarned,
                Source = "Quiz",
                CreatedAt = DateTime.Now
            });

            // Cập nhật tổng XP của học sinh
            var profile = await _db.StudentProfiles!.FindAsync(studentId);

            if (profile != null)
                profile.XP += xpEarned;
        }

        await _db.SaveChangesAsync();
    }
}
