namespace EnglishLearningOnlineSystem.Models;

public class WeeklyAssignmentMiniGame
{
    public int AssignmentId { get; set; }
    public WeeklyAssignment Assignment { get; set; } = null!;

    public int GameId { get; set; }
    public MiniGame MiniGame { get; set; } = null!;
}
