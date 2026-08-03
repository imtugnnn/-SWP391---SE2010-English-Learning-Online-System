namespace EnglishLearningOnlineSystem.ViewModels;

public class StudentDashboardViewModel
{
    // Profile summary
    public string Nickname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Level { get; set; }
    public int XP { get; set; }
    public int XPToNextLevel { get; set; }
    public int CurrentStreakDays { get; set; }
    public int LongestStreak { get; set; }

    // Assigned lessons this week
    public List<AssignedLessonSummary> AssignedLessons { get; set; } = new();

    // Recent progress
    public List<RecentProgressSummary> RecentProgress { get; set; } = new();

    // Daily missions
    public List<DailyMissionSummary> DailyMissions { get; set; } = new();

    // Badges earned
    public List<BadgeSummary> RecentBadges { get; set; } = new();

    // Adaptive Learning
    public List<LessonRecommendation> Recommendations { get; set; } = new();

    // Stats
    public int TotalLessonsCompleted { get; set; }
    public int TotalXPEarned { get; set; }

    // Onboarding
    public bool IsFirstLogin { get; set; }
}

public class AssignedLessonSummary
{
    public int AssignmentId { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime DueDate { get; set; }
    public string CompletionStatus { get; set; } = "NOT_STARTED"; // NOT_STARTED / IN_PROGRESS / COMPLETED
}

public class RecentProgressSummary
{
    public string LessonTitle { get; set; } = string.Empty;
    public int QuizScore { get; set; }
    public int XPEarned { get; set; }
    public DateTime CompletedAt { get; set; }
    public string CompletionStatus { get; set; } = string.Empty;
}

public class DailyMissionSummary
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int CurrentValue { get; set; }
    public int TargetValue { get; set; }
    public int XPReward { get; set; }
    public bool IsCompleted { get; set; }
}

public class BadgeSummary
{
    public string BadgeName { get; set; } = string.Empty;
    public string IconUrl { get; set; } = string.Empty;
    public DateTime EarnedAt { get; set; }
}
