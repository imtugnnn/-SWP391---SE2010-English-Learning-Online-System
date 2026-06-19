namespace EnglishLearningOnlineSystem.ViewModels;

public class ParentDashboardViewModel
{
    public bool HasLinkedChildren { get; set; }
    public bool LoadFailed { get; set; }

    public int SelectedStudentId { get; set; }
    public List<ChildOption> Children { get; set; } = new();

    public ChildLearningOverview? Overview { get; set; }
}

public class ChildOption
{
    public int StudentId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Relationship { get; set; }
    public bool IsSelected { get; set; }
}

public class ChildLearningOverview
{
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? StudentCode { get; set; }

    public int Level { get; set; }
    public int XP { get; set; }
    public int CurrentStreakDays { get; set; }
    public int LessonsCompleted { get; set; }
    public int BadgesEarned { get; set; }
    public int AverageQuizScore { get; set; }
    public DateTime? LastActiveDate { get; set; }

    public bool HasLearningData { get; set; }

    public List<ParentRecentActivity> RecentActivities { get; set; } = new();
    public List<ParentUpcomingTask> UpcomingTasks { get; set; } = new();
    public List<ParentBadgeItem> RecentBadges { get; set; } = new();
}

public class ParentRecentActivity
{
    public string LessonTitle { get; set; } = string.Empty;
    public int QuizScore { get; set; }
    public int XPEarned { get; set; }
    public string CompletionStatus { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
}

public class ParentUpcomingTask
{
    public string LessonTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public DateTime DueDate { get; set; }
}

public class ParentBadgeItem
{
    public string BadgeName { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public DateTime EarnedAt { get; set; }
}
