namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherStudentDetailViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    public int Level { get; set; }
    public int XP { get; set; }
    public int CurrentStreakDays { get; set; }
    public DateTime? LastActiveDate { get; set; }

    public int CompletedLessons { get; set; }
    public int InProgressLessons { get; set; }
    public double AverageQuizScore { get; set; }
    public int TotalXPEarned { get; set; }
    public int StudyDurationMinutes { get; set; }

    public List<TeacherStudentLessonProgressViewModel> LessonProgresses { get; set; } = new();
    public List<TeacherStudentFeedbackViewModel> Feedbacks { get; set; } = new();
}

public class TeacherStudentLessonProgressViewModel
{
    public int? AssignmentId { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int QuizScore { get; set; }
    public int XPEarned { get; set; }
    public string CompletionStatus { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string FlashcardStatus { get; set; } = "NotAssigned";
    public string QuizStatus { get; set; } = "NotAssigned";
    public string MiniGameStatus { get; set; } = "NotAssigned";
    public bool IsCompletedLate { get; set; }
}

public class TeacherStudentFeedbackViewModel
{
    public int FeedbackId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public string ReadStatus { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; }
}
