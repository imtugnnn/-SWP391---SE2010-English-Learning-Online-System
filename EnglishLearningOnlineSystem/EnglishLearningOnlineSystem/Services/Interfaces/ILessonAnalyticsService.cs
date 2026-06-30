using EnglishLearningOnlineSystem.ViewModels.ContentManager.LessonAnalytics;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ILessonAnalyticsService
{
    /// <summary>Dashboard: summary rows cho tất cả bài học, lọc theo khóa học/tên bài học
    /// và sắp xếp theo "students_desc", "score_desc", "score_asc", "xp_desc", "title_asc".</summary>
    Task<LessonAnalyticsDashboardViewModel> GetDashboardAsync(
        int? courseId = null,
        string? search = null,
        string? sortBy = null);

    /// <summary>Detailed analytics cho 1 bài học. <paramref name="days"/> là khoảng ngày cho
    /// chart lượt làm bài (7, 30 hoặc 90 — giá trị khác sẽ mặc định về 30).</summary>
    Task<LessonAnalyticsDetailViewModel?> GetDetailAsync(int lessonId, int days = 30);
}