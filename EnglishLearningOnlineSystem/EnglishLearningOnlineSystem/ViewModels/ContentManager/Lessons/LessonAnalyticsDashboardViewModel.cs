namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.LessonAnalytics;

// ── Shared ────────────────────────────────────────────────────────────────────

public class CourseFilterItem
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
}

// ── Dashboard (all lessons) ───────────────────────────────────────────────────

public class LessonAnalyticsDashboardViewModel
{
    public List<LessonAnalyticsSummaryViewModel> Items { get; set; } = [];
    public List<CourseFilterItem> Courses { get; set; } = [];
    public int? FilterCourseId { get; set; }
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }

    // Top KPI bar
    public int TotalLessons { get; set; }
    public int TotalStudentsAll { get; set; }     // tổng lượt tham gia (1 học viên có thể tính ở nhiều bài)
    public int TotalUniqueStudents { get; set; }  // học viên KHÔNG trùng lặp
    public double OverallAvgScore { get; set; }   // trung bình có trọng số theo số lượt làm bài
    public int TotalXpAll { get; set; }
}

// ── Per-lesson summary row (used in dashboard table) ─────────────────────────

public class LessonAnalyticsSummaryViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public bool IsPublished { get; set; }
    public int EstimatedMinutes { get; set; }

    public int TotalStudents { get; set; }
    public double AvgQuizScore { get; set; }
    public double FlashcardCompletionRate { get; set; }
    public int TotalXpAwarded { get; set; }

    public string ScoreBadgeClass => AvgQuizScore switch
    {
        >= 85 => "bg-success-subtle text-success border-success-subtle",
        >= 70 => "bg-info-subtle text-info border-info-subtle",
        >= 50 => "bg-warning-subtle text-warning border-warning-subtle",
        _ => "bg-danger-subtle text-danger border-danger-subtle"
    };
}

// ── Detailed analytics for a single lesson ────────────────────────────────────

public class LessonAnalyticsDetailViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    public int TotalStudents { get; set; }
    public double AvgQuizScore { get; set; }
    public double FlashcardCompletionRate { get; set; }
    public double FlashcardAccuracyRate { get; set; }
    public int TotalXpAwarded { get; set; }
    public double AvgStudyMinutes { get; set; }
    public int TotalQuizAttempts { get; set; }
    public int TotalFlashcardSessions { get; set; }

    public int SelectedRangeDays { get; set; } = 30;

    public string ScoreBadgeClass => AvgQuizScore switch
    {
        >= 85 => "bg-success-subtle text-success border-success-subtle",
        >= 70 => "bg-info-subtle text-info border-info-subtle",
        >= 50 => "bg-warning-subtle text-warning border-warning-subtle",
        _ => "bg-danger-subtle text-danger border-danger-subtle"
    };

    public Dictionary<string, int> DailyAttemptCounts { get; set; } = [];
    public Dictionary<string, int> ScoreDistribution { get; set; } = [];
}