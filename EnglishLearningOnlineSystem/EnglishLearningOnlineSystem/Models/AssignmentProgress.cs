using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models;

public enum AssignmentCompletionStatus
{
    NotStarted,
    InProgress,
    Completed
}

public class AssignmentProgress
{
    [Key]
    public int AssignmentProgressId { get; set; }

    public int AssignmentId { get; set; }
    [ForeignKey(nameof(AssignmentId))]
    public WeeklyAssignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    [ForeignKey(nameof(StudentId))]
    public StudentProfile Student { get; set; } = null!;

    public AssignmentCompletionStatus Status { get; set; } = AssignmentCompletionStatus.NotStarted;
    public int CompletedActivityCount { get; set; }
    public int RequiredActivityCount { get; set; }
    public int? BestQuizScore { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompletedLate { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
