using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.LessonAnalytics;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class LessonAnalyticsService : ILessonAnalyticsService
{
    private readonly ILessonAnalyticsRepository _analyticsRepo;
    private readonly AppDbContext _db;

    public LessonAnalyticsService(ILessonAnalyticsRepository analyticsRepo, AppDbContext db)
    {
        _analyticsRepo = analyticsRepo;
        _db = db;
    }

    public async Task<LessonAnalyticsDashboardViewModel> GetDashboardAsync(int? courseId = null)
    {
        var rows = (await _analyticsRepo.GetAllLessonsSummaryAsync(courseId)).ToList();

        var courses = await _db.Courses!
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderBy(c => c.CourseName)
            .Select(c => new CourseFilterItem { CourseId = c.CourseId, CourseName = c.CourseName })
            .ToListAsync();

        var items = rows.Select(r => new LessonAnalyticsSummaryViewModel
        {
            LessonId = r.LessonId,
            Title = r.Title,
            CourseName = r.CourseName,
            CourseId = r.CourseId,
            IsPublished = r.IsPublished,
            EstimatedMinutes = r.EstimatedMinutes,
            TotalStudents = r.TotalStudents,
            AvgQuizScore = r.AvgQuizScore,
            FlashcardCompletionRate = r.FlashcardCompletionRate,
            TotalXpAwarded = r.TotalXpAwarded
        }).ToList();

        return new LessonAnalyticsDashboardViewModel
        {
            Items = items,
            Courses = courses,
            FilterCourseId = courseId,
            // Top-level KPIs
            TotalLessons = items.Count,
            TotalStudentsAll = rows.Sum(r => r.TotalStudents),
            OverallAvgScore = rows.Any() ? Math.Round(rows.Average(r => r.AvgQuizScore), 1) : 0,
            TotalXpAll = rows.Sum(r => r.TotalXpAwarded)
        };
    }

    public async Task<LessonAnalyticsDetailViewModel?> GetDetailAsync(int lessonId)
    {
        var lesson = await _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson == null) return null;

        var totalStudents = await _analyticsRepo.GetTotalStudentsAsync(lessonId);
        var avgQuizScore = await _analyticsRepo.GetAverageQuizScoreAsync(lessonId);
        var flashcardCompletionRate = await _analyticsRepo.GetFlashcardCompletionRateAsync(lessonId);
        var totalXp = await _analyticsRepo.GetTotalXpAwardedAsync(lessonId);
        var avgStudyMinutes = await _analyticsRepo.GetAverageStudyMinutesAsync(lessonId);
        var dailyCounts = await _analyticsRepo.GetDailyAttemptCountsAsync(lessonId, 30);
        var scoreDist = await _analyticsRepo.GetScoreDistributionAsync(lessonId);

        // Total quiz attempts count
        var totalAttempts = await _db.QuizAttempts
            .AsNoTracking()
            .CountAsync(qa => qa.LessonId == lessonId);

        // Total flashcard sessions
        var totalFlashcardSessions = await _db.FlashcardSessions
            .AsNoTracking()
            .CountAsync(fs => fs.LessonId == lessonId);

        return new LessonAnalyticsDetailViewModel
        {
            LessonId = lesson.LessonId,
            Title = lesson.Title,
            Topic = lesson.Topic,
            IsPublished = lesson.IsPublished,
            EstimatedMinutes = lesson.EstimatedMinutes,
            XPReward = lesson.XPReward,
            CourseId = lesson.CourseId,
            CourseName = lesson.Course?.CourseName ?? "—",

            // KPI cards
            TotalStudents = totalStudents,
            AvgQuizScore = avgQuizScore,
            FlashcardCompletionRate = flashcardCompletionRate,
            TotalXpAwarded = totalXp,
            AvgStudyMinutes = avgStudyMinutes,
            TotalQuizAttempts = totalAttempts,
            TotalFlashcardSessions = totalFlashcardSessions,

            // Chart data
            DailyAttemptCounts = dailyCounts,
            ScoreDistribution = scoreDist
        };
    }
}