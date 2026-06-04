namespace EnglishLearningOnlineSystem.ViewModels;

public class LessonRecommendation
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public int Mastery { get; set; } // 0-100
    public string Priority { get; set; } = "C"; // A, B, C
    public string ActionUrl { get; set; } = string.Empty;
}
