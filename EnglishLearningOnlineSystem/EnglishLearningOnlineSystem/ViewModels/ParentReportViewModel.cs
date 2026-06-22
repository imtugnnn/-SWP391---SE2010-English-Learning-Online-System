namespace EnglishLearningOnlineSystem.ViewModels;

public class ParentReportViewModel
{
    public bool HasLinkedChildren { get; set; }
    public bool LoadFailed { get; set; }
    public bool HasReportData { get; set; }

    public int SelectedStudentId { get; set; }
    public List<ChildOption> Children { get; set; } = new();

    public string Period { get; set; } = "week";
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }

    public string ChildDisplayName { get; set; } = string.Empty;
    public string? ChildAvatarUrl { get; set; }

    public int LessonsCompleted { get; set; }
    public int QuizzesTaken { get; set; }
    public int AverageQuizScore { get; set; }
    public int XPEarnedInPeriod { get; set; }
    public int TotalTimeSpentMinutes { get; set; }

    public List<SkillProgressItem> SkillProgress { get; set; } = new();
    public List<QuizResultItem> QuizResults { get; set; } = new();
    public List<TeacherFeedbackItem> Feedbacks { get; set; } = new();
}

public class SkillProgressItem
{
    public string Topic { get; set; } = string.Empty;
    public int AverageScore { get; set; }
    public int AttemptCount { get; set; }
}

public class QuizResultItem
{
    public string LessonTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public int TimeSpentSec { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class TeacherFeedbackItem
{
    public string Content { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
