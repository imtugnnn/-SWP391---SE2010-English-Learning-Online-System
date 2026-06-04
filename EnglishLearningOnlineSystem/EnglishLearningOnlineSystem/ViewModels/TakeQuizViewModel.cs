namespace EnglishLearningOnlineSystem.ViewModels;

public class TakeQuizViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int? WeeklyAssignmentId { get; set; }
    public bool IsOverdue { get; set; }
    public List<QuizQuestionItem> Questions { get; set; } = new();
}

public class QuizQuestionItem
{
    public int QuizId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuizType { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
}
