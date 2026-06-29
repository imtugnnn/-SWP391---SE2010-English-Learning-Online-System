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

    public async Task<LessonAnalyticsDashboardViewModel> GetDashboardAsync(
        int? courseId = null,
        string? search = null,
        string? sortBy = null)
    {
        var rows = (await _analyticsRepo.GetAllLessonsSummaryAsync(courseId, search, sortBy)).ToList();

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

        // Tính trên dữ liệu thô (đúng trọng số theo số lượt làm bài), không phải
        // trung bình của các trung bình per-lesson.
        var lessonIds = rows.Select(r => r.LessonId).ToList();
        var (weightedAvgScore, uniqueStudents) = await _analyticsRepo.GetOverallStatsAsync(lessonIds);

        return new LessonAnalyticsDashboardViewModel
        {
            Items = items,
            Courses = courses,
            FilterCourseId = courseId,
            SearchTerm = search,
            SortBy = sortBy,

            TotalLessons = items.Count,
            TotalStudentsAll = rows.Sum(r => r.TotalStudents),
            TotalUniqueStudents = uniqueStudents,
            OverallAvgScore = weightedAvgScore,
            TotalXpAll = rows.Sum(r => r.TotalXpAwarded)
        };
    }

    public async Task<LessonAnalyticsDetailViewModel?> GetDetailAsync(int lessonId, int days = 30)
    {
        if (days != 7 && days != 30 && days != 90) days = 30;

        var lesson = await _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson == null) return null;

        var core = await _analyticsRepo.GetLessonCoreStatsAsync(lessonId);
        var flashcardCompletionRate = await _analyticsRepo.GetFlashcardCompletionRateAsync(lessonId);
        var flashcardAccuracyRate = await _analyticsRepo.GetFlashcardAccuracyRateAsync(lessonId);
        var avgStudyMinutes = await _analyticsRepo.GetAverageStudyMinutesAsync(lessonId);
        var dailyCounts = await _analyticsRepo.GetDailyAttemptCountsAsync(lessonId, days);
        var scoreDist = await _analyticsRepo.GetScoreDistributionAsync(lessonId);

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

            TotalStudents = core.TotalStudents,
            AvgQuizScore = core.AvgQuizScore,
            FlashcardCompletionRate = flashcardCompletionRate,
            FlashcardAccuracyRate = flashcardAccuracyRate,
            TotalXpAwarded = core.TotalXpAwarded,
            AvgStudyMinutes = avgStudyMinutes,
            TotalQuizAttempts = core.TotalQuizAttempts,
            TotalFlashcardSessions = totalFlashcardSessions,

            DailyAttemptCounts = dailyCounts,
            ScoreDistribution = scoreDist,
            SelectedRangeDays = days
        };
    }
}