namespace EnglishLearningOnlineSystem.Models;

public class WeeklyAssignmentVocabulary
{
    public int AssignmentId { get; set; }
    public WeeklyAssignment Assignment { get; set; } = null!;

    public int VocabularyId { get; set; }
    public Vocabulary Vocabulary { get; set; } = null!;
}
