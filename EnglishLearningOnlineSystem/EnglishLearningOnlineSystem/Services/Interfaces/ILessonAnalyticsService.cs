using EnglishLearningOnlineSystem.ViewModels.ContentManager.LessonAnalytics;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ILessonAnalyticsService
{
    /// <summary>Dashboard: summary rows for all lessons (optionally filtered by course).</summary>
    Task<LessonAnalyticsDashboardViewModel> GetDashboardAsync(int? courseId = null);

    /// <summary>Detailed analytics for a single lesson.</summary>
    Task<LessonAnalyticsDetailViewModel?> GetDetailAsync(int lessonId);
}