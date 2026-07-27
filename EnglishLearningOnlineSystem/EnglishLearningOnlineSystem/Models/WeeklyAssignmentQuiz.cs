namespace EnglishLearningOnlineSystem.Models;

public class WeeklyAssignmentQuiz
{
    public int AssignmentId { get; set; }
    public WeeklyAssignment Assignment { get; set; } = null!;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;
}
