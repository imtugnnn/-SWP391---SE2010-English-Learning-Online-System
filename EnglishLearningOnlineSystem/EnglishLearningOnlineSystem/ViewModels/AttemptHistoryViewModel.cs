namespace EnglishLearningOnlineSystem.ViewModels;

public class AttemptHistoryViewModel
{
    public List<AttemptSummaryItem> Attempts { get; set; } = new();
    public int? FilterLessonId { get; set; }
    public string? FilterFrom { get; set; }
    public string? FilterTo { get; set; }
    public string SortBy { get; set; } = "date";
    public List<LessonFilterItem> AvailableLessons { get; set; } = new();

    public string Nickname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Level { get; set; }
    public int XP { get; set; }
    public int CurrentStreakDays { get; set; }
}

public class AttemptSummaryItem
{
    public int AttemptId { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeSpentSec { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool XpAwarded { get; set; }
}

public class LessonFilterItem
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
}
