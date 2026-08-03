using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IAssignmentProgressService
{
    Task<AssignmentProgressSnapshot?> GetSnapshotAsync(int assignmentId, int studentId);
    Task<bool> MarkActivityStartedAsync(
        int assignmentId,
        int studentId,
        AssignmentActivityType activityType,
        int? activityId = null);
    Task<bool> MarkActivityCompletedAsync(
        int assignmentId,
        int studentId,
        AssignmentActivityType activityType,
        int? activityId = null,
        int? score = null);
}

public sealed class AssignmentProgressSnapshot
{
    public AssignmentCompletionStatus Status { get; init; }
    public int CompletedActivityCount { get; init; }
    public int RequiredActivityCount { get; init; }
    public int? BestQuizScore { get; init; }
    public DateTime? CompletedAt { get; init; }
    public bool IsCompletedLate { get; init; }
    public IReadOnlyDictionary<AssignmentActivityType, AssignmentActivityStatus?> ActivityStatuses { get; init; }
        = new Dictionary<AssignmentActivityType, AssignmentActivityStatus?>();
}
