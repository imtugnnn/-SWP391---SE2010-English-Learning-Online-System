using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models;

public enum AssignmentActivityType
{
    Flashcard,
    Quiz,
    MiniGame
}

public enum AssignmentActivityStatus
{
    InProgress,
    Completed
}

public class AssignmentActivityProgress
{
    [Key]
    public int ActivityProgressId { get; set; }

    public int AssignmentId { get; set; }
    [ForeignKey(nameof(AssignmentId))]
    public WeeklyAssignment Assignment { get; set; } = null!;

    public int StudentId { get; set; }
    [ForeignKey(nameof(StudentId))]
    public StudentProfile Student { get; set; } = null!;

    public AssignmentActivityType ActivityType { get; set; }

    // Flashcard và Quiz là hoạt động tổng hợp nên dùng ActivityId = 0;
    // Mini-game dùng GameId. Khóa không-null giúp unique index chặn request trùng thật sự.
    public int ActivityId { get; set; }
    public AssignmentActivityStatus Status { get; set; } = AssignmentActivityStatus.InProgress;
    public int? Score { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
