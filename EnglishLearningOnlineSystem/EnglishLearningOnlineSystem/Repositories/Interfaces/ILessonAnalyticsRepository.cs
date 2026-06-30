namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface ILessonAnalyticsRepository
{
    // ── Per-lesson raw metrics ─────────────────────────────────────────────────

    /// <summary>Total distinct students who submitted at least one quiz attempt for this lesson.</summary>
    Task<int> GetTotalStudentsAsync(int lessonId);

    /// <summary>Average score (0–100) across all quiz attempts for this lesson.</summary>
    Task<double> GetAverageQuizScoreAsync(int lessonId);

    /// <summary>Ratio of flashcard sessions where all cards were answered correctly (0–1).</summary>
    Task<double> GetFlashcardCompletionRateAsync(int lessonId);

    /// <summary>Total XP awarded through quiz attempts linked to this lesson.</summary>
    Task<int> GetTotalXpAwardedAsync(int lessonId);

    /// <summary>
    /// Average time in minutes students spent across all quiz attempts for this lesson.
    /// Derived from QuizAttempt.SubmittedAt and the lesson EstimatedMinutes as a fallback.
    /// </summary>
    Task<double> GetAverageStudyMinutesAsync(int lessonId);

    /// <summary>Quiz attempt count per day for the last <paramref name="days"/> days.</summary>
    Task<Dictionary<string, int>> GetDailyAttemptCountsAsync(int lessonId, int days = 30);

    /// <summary>Score distribution buckets: 0-49, 50-69, 70-84, 85-100.</summary>
    Task<Dictionary<string, int>> GetScoreDistributionAsync(int lessonId);

    // ── Cross-lesson dashboard metrics ────────────────────────────────────────

    /// <summary>
    /// Returns per-lesson summary rows for all lessons belonging to <paramref name="courseId"/>
    /// (or all lessons if null). Each row contains LessonId, Title, student count, avg score,
    /// flashcard completion rate, total XP.
    /// </summary>
    Task<IEnumerable<LessonAnalyticsRowData>> GetAllLessonsSummaryAsync(int? courseId = null);
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