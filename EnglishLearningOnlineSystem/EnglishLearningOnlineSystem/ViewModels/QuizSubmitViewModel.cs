namespace EnglishLearningOnlineSystem.ViewModels;

public class QuizSubmitViewModel
{
    public int LessonId { get; set; }
    public int? WeeklyAssignmentId { get; set; }
    public long StartedAtTicks { get; set; }
    public Dictionary<int, string> Answers { get; set; } = new(); // QuizId -> SelectedAnswer
}
