namespace EnglishLearningOnlineSystem.ViewModels;

public class StudentNotificationCenterViewModel
{
    public string Filter { get; set; } = "all";
    public int UnreadCount { get; set; }
    public List<StudentNotificationItemViewModel> Notifications { get; set; } = new();
}

public class StudentNotificationItemViewModel
{
    public int NotificationId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
    public string? TargetUrl { get; set; }
}

public class StudentTeacherFeedbackViewModel
{
    public int UnreadCount { get; set; }
    public List<StudentTeacherFeedbackItemViewModel> Feedbacks { get; set; } = new();
}

public class StudentTeacherFeedbackItemViewModel
{
    public int FeedbackId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string? AssignmentTitle { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
