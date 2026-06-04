using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class AdaptiveLearningService : IAdaptiveLearningService
{
    private readonly AppDbContext _db;

    public AdaptiveLearningService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<LessonRecommendation>> GetSuggestionsAsync(int studentId)
    {
        var suggestions = new List<LessonRecommendation>();

        // 1.Tìm bài tập sắp đến hạn
        var urgentAssignments = await _db.WeeklyAssignments!
            .Include(wa => wa.Lesson!)
            .ThenInclude(l => l.Course!)
            .Where(wa => wa.IsVisible && wa.DueDate >= DateTime.Today)
            .ToListAsync();

        var completedLessonIds = await _db.Progresses!
            .Where(p => p.StudentId == studentId && p.CompletionStatus == "Completed")
            .Select(p => p.LessonId)
            .ToListAsync();

        foreach (var assignment in urgentAssignments.Where(a => !completedLessonIds.Contains(a.LessonId ?? 0)))
        {
            int daysLeft = (assignment.DueDate.Date - DateTime.Today).Days;
            if (daysLeft <= 3 && assignment.Lesson != null)
            {
                suggestions.Add(new LessonRecommendation
                {
                    LessonId = assignment.Lesson.LessonId,
                    LessonTitle = assignment.Lesson.Title,
                    CourseName = assignment.Lesson.Course?.CourseName ?? "Course",
                    Reason = $"Bài tập sẽ hết hạn sau {daysLeft} ngày!",
                    Mastery = 0,
                    Priority = "A",
                    ActionUrl = $"/student/lesson/{assignment.Lesson.LessonId}/quiz"
                });
            }
        }

        // 2.Xác định bài tập có tỉ lệ flashcard recall < 70%
        var recentSessions = await _db.FlashcardSessions
            .Include(s => s.Lesson!)
            .ThenInclude(l => l.Course!)
            .Include(s => s.CardResults)
            .Where(s => s.StudentId == studentId && s.CompletedAt != null)
            .OrderByDescending(s => s.CompletedAt)
            .Take(10)
            .ToListAsync();

        var lessonRecall = recentSessions
            .GroupBy(s => s.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                Session = g.First(), // Session gần nhất
                TotalCards = g.First().CardResults.Count,
                KnewCards = g.First().CardResults.Count(cr => cr.KnewIt)
            })
            .Where(x => x.TotalCards > 0)
            .ToList();

        foreach (var stat in lessonRecall)
        {
            double recallPercent = (double)stat.KnewCards / stat.TotalCards * 100;
            if (recallPercent < 70 && stat.Session.Lesson != null)
            {
                suggestions.Add(new LessonRecommendation
                {
                    LessonId = stat.LessonId,
                    LessonTitle = stat.Session.Lesson.Title,
                    CourseName = stat.Session.Lesson.Course?.CourseName ?? "Course",
                    Reason = $"Tỉ lệ nhớ từ vựng thấp ({(int)recallPercent}%). Nên ôn tập lại.",
                    Mastery = (int)recallPercent,
                    Priority = "B",
                    ActionUrl = $"/student/lesson/{stat.LessonId}/flashcards"
                });
            }
        }

        // 3.Xác định bài tập có điểm quiz < 80%
        var bestScores = await _db.Progresses!
            .Include(p => p.Lesson!)
            .ThenInclude(l => l.Course!)
            .Where(p => p.StudentId == studentId && p.IsBestAttempt)
            .ToListAsync();

        foreach (var progress in bestScores.Where(p => p.QuizScore < 80))
        {
            if (progress.Lesson != null && !suggestions.Any(s => s.LessonId == progress.LessonId))
            {
                suggestions.Add(new LessonRecommendation
                {
                    LessonId = progress.LessonId,
                    LessonTitle = progress.Lesson.Title,
                    CourseName = progress.Lesson.Course?.CourseName ?? "Course",
                    Reason = $"Cải thiện điểm bài kiểm tra (hiện tại: {progress.QuizScore}%).",
                    Mastery = progress.QuizScore,
                    Priority = "C",
                    ActionUrl = $"/student/lesson/{progress.LessonId}/quiz"
                });
            }
        }

        return suggestions
            .OrderBy(s => s.Priority)
            .ThenBy(s => s.Mastery)
            .Take(5)
            .ToList();
    }
}
