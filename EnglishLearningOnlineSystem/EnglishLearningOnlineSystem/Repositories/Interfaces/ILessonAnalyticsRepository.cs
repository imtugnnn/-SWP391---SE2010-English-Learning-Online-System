namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface ILessonAnalyticsRepository
{
    // ── Per-lesson raw metrics ─────────────────────────────────────────────────

    /// <summary>Gộp các chỉ số cốt lõi của 1 bài học trong 1 query duy nhất: số học viên,
    /// điểm trung bình, tổng XP, tổng lượt làm bài.</summary>
    Task<LessonCoreStatsData> GetLessonCoreStatsAsync(int lessonId);

    /// <summary>Total distinct students who submitted at least one quiz attempt for this lesson.</summary>
    Task<int> GetTotalStudentsAsync(int lessonId);

    /// <summary>Average score (0–100) across all quiz attempts for this lesson.</summary>
    Task<double> GetAverageQuizScoreAsync(int lessonId);

    /// <summary>Tỉ lệ buổi flashcard mà TẤT CẢ thẻ đều được trả lời đúng (0–100).
    /// Đây là chỉ số "mastery" (làm đúng hết), không phải "đã hoàn thành buổi".</summary>
    Task<double> GetFlashcardCompletionRateAsync(int lessonId);

    /// <summary>% số thẻ trả lời đúng trên TỔNG số thẻ đã học, tính trên toàn bộ session (0–100).
    /// Khác với <see cref="GetFlashcardCompletionRateAsync"/>, chỉ số này cho điểm từng phần,
    /// không bị "tất cả hoặc không gì" chỉ vì sai 1 thẻ trong 1 buổi học tốt.</summary>
    Task<double> GetFlashcardAccuracyRateAsync(int lessonId);

    /// <summary>Total XP awarded through quiz attempts linked to this lesson.</summary>
    Task<int> GetTotalXpAwardedAsync(int lessonId);

    /// <summary>
    /// Average time in minutes students spent across all quiz attempts for this lesson.
    /// Falls back to the lesson's EstimatedMinutes when no attempt has a recorded time.
    /// </summary>
    Task<double> GetAverageStudyMinutesAsync(int lessonId);

    /// <summary>Quiz attempt count per day for the last <paramref name="days"/> days.</summary>
    Task<Dictionary<string, int>> GetDailyAttemptCountsAsync(int lessonId, int days = 30);

    /// <summary>Score distribution buckets: 0-49, 50-69, 70-84, 85-100.</summary>
    Task<Dictionary<string, int>> GetScoreDistributionAsync(int lessonId);

    // ── Cross-lesson dashboard metrics ────────────────────────────────────────

    /// <summary>
    /// Per-lesson summary rows, lọc theo <paramref name="courseId"/> (null = tất cả),
    /// tìm theo tên bài học (<paramref name="search"/>) và sắp xếp theo
    /// <paramref name="sortBy"/>: "students_desc", "score_desc", "score_asc", "xp_desc",
    /// "title_asc" — null/không hợp lệ giữ thứ tự mặc định theo khóa học/order index.
    /// </summary>
    Task<IEnumerable<LessonAnalyticsRowData>> GetAllLessonsSummaryAsync(
        int? courseId = null,
        string? search = null,
        string? sortBy = null);

    /// <summary>
    /// Chỉ số tổng quan tính TRỰC TIẾP trên dữ liệu lượt làm bài thô (không phải "trung bình
    /// của các trung bình" theo từng bài học), nên trọng số phản ánh đúng số lượt làm bài thật.
    /// Trả về điểm trung bình có trọng số và số học viên KHÔNG trùng lặp.
    /// </summary>
    Task<(double WeightedAvgScore, int UniqueStudents)> GetOverallStatsAsync(IEnumerable<int> lessonIds);
}

/// <summary>Raw aggregate row used internally by the repository and service layers.</summary>
public record LessonAnalyticsRowData(
    int LessonId,
    string Title,
    string CourseName,
    int CourseId,
    bool IsPublished,
    int EstimatedMinutes,
    int TotalStudents,
    double AvgQuizScore,
    double FlashcardCompletionRate,
    int TotalXpAwarded
);

/// <summary>Chỉ số cốt lõi của 1 bài học, lấy trong 1 round-trip duy nhất.</summary>
public record LessonCoreStatsData(
    int TotalStudents,
    double AvgQuizScore,
    int TotalXpAwarded,
    int TotalQuizAttempts
);