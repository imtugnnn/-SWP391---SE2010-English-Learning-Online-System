namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;

public class LessonListItemViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public bool IsPublished { get; set; }
    public string CourseName { get; set; } = string.Empty;
}
