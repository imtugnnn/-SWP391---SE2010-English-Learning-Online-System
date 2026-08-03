using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class AssignmentProgressService : IAssignmentProgressService
{
    private readonly IAssignmentProgressRepository _repository;

    public AssignmentProgressService(IAssignmentProgressRepository repository)
    {
        _repository = repository;
    }

    public async Task<AssignmentProgressSnapshot?> GetSnapshotAsync(int assignmentId, int studentId)
    {
        var assignment = await _repository.GetAccessibleAssignmentAsync(assignmentId, studentId);
        if (assignment == null) return null;

        var progress = await _repository.GetProgressAsync(assignmentId, studentId);
        var activities = await _repository.GetActivityProgressesAsync(assignmentId, studentId);
        return BuildSnapshot(assignment, progress, activities);
    }

    public Task<bool> MarkActivityStartedAsync(
        int assignmentId,
        int studentId,
        AssignmentActivityType activityType,
        int? activityId = null)
    {
        return RecordActivityAsync(assignmentId, studentId, activityType, activityId, false, null);
    }

    public Task<bool> MarkActivityCompletedAsync(
        int assignmentId,
        int studentId,
        AssignmentActivityType activityType,
        int? activityId = null,
        int? score = null)
    {
        return RecordActivityAsync(assignmentId, studentId, activityType, activityId, true, score);
    }

    private async Task<bool> RecordActivityAsync(
        int assignmentId,
        int studentId,
        AssignmentActivityType activityType,
        int? activityId,
        bool completed,
        int? score)
    {
        var assignment = await _repository.GetAccessibleAssignmentAsync(assignmentId, studentId);
        if (assignment == null || !IsConfiguredActivity(assignment, activityType, activityId))
        {
            return false;
        }

        var now = DateTime.UtcNow;
        var normalizedActivityId = activityId ?? 0;
        var progress = await _repository.GetProgressAsync(assignmentId, studentId);
        if (progress == null)
        {
            progress = new AssignmentProgress
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                Status = AssignmentCompletionStatus.InProgress,
                StartedAt = now,
                UpdatedAt = now
            };
            await _repository.AddProgressAsync(progress);
        }

        var activities = await _repository.GetActivityProgressesAsync(assignmentId, studentId);
        var activity = activities.FirstOrDefault(x =>
            x.ActivityType == activityType && x.ActivityId == normalizedActivityId);
        if (activity == null)
        {
            activity = new AssignmentActivityProgress
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                ActivityType = activityType,
                ActivityId = normalizedActivityId,
                StartedAt = now
            };
            activities.Add(activity);
            await _repository.AddActivityProgressAsync(activity);
        }

        // Business process: kết quả activity chỉ tiến tới Completed, không được lùi trạng thái.
        if (completed)
        {
            activity.Status = AssignmentActivityStatus.Completed;
            activity.CompletedAt ??= now;
            activity.Score = score.HasValue
                ? Math.Max(activity.Score ?? 0, score.Value)
                : activity.Score;
        }
        activity.UpdatedAt = now;

        RecalculateAssignmentProgress(assignment, progress, activities, now);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static void RecalculateAssignmentProgress(
        WeeklyAssignment assignment,
        AssignmentProgress progress,
        List<AssignmentActivityProgress> activities,
        DateTime now)
    {
        var requiredKeys = GetRequiredActivityKeys(assignment);
        var completedKeys = activities
            .Where(x => x.Status == AssignmentActivityStatus.Completed)
            .Select(x => (x.ActivityType, x.ActivityId))
            .ToHashSet();

        progress.RequiredActivityCount = requiredKeys.Count;
        progress.CompletedActivityCount = requiredKeys.Count(completedKeys.Contains);
        progress.BestQuizScore = activities
            .Where(x => x.ActivityType == AssignmentActivityType.Quiz)
            .Max(x => x.Score);
        progress.Status = progress.RequiredActivityCount > 0 &&
                          progress.CompletedActivityCount == progress.RequiredActivityCount
            ? AssignmentCompletionStatus.Completed
            : AssignmentCompletionStatus.InProgress;

        if (progress.Status == AssignmentCompletionStatus.Completed && !progress.CompletedAt.HasValue)
        {
            // Nộp sau hạn vẫn được ghi nhận, đồng thời tách riêng Completed Late để Teacher hỗ trợ.
            progress.CompletedAt = now;
            progress.IsCompletedLate = now > assignment.DueDate;
        }
        progress.UpdatedAt = now;
    }

    private static bool IsConfiguredActivity(
        WeeklyAssignment assignment,
        AssignmentActivityType activityType,
        int? activityId)
    {
        return activityType switch
        {
            AssignmentActivityType.Flashcard =>
                activityId == null && assignment.IncludeVocabulary && assignment.Vocabularies.Count > 0,
            AssignmentActivityType.Quiz =>
                activityId == null && assignment.IncludeQuiz && assignment.Quizzes.Count > 0,
            AssignmentActivityType.MiniGame =>
                activityId.HasValue && assignment.IncludeMiniGame &&
                assignment.MiniGames.Any(x => x.GameId == activityId.Value),
            _ => false
        };
    }

    private static List<(AssignmentActivityType ActivityType, int ActivityId)> GetRequiredActivityKeys(
        WeeklyAssignment assignment)
    {
        var keys = new List<(AssignmentActivityType, int)>();
        if (assignment.IncludeVocabulary && assignment.Vocabularies.Count > 0)
            keys.Add((AssignmentActivityType.Flashcard, 0));
        if (assignment.IncludeQuiz && assignment.Quizzes.Count > 0)
            keys.Add((AssignmentActivityType.Quiz, 0));
        if (assignment.IncludeMiniGame)
            keys.AddRange(assignment.MiniGames.Select(x => (AssignmentActivityType.MiniGame, x.GameId)));
        return keys;
    }

    private static AssignmentProgressSnapshot BuildSnapshot(
        WeeklyAssignment assignment,
        AssignmentProgress? progress,
        List<AssignmentActivityProgress> activities)
    {
        var required = GetRequiredActivityKeys(assignment);
        AssignmentActivityStatus? Aggregate(AssignmentActivityType type)
        {
            var keys = required.Where(x => x.ActivityType == type).ToList();
            if (keys.Count == 0) return null;
            var records = activities.Where(x => x.ActivityType == type).ToList();
            if (keys.All(k => records.Any(x => x.ActivityId == k.ActivityId &&
                                                x.Status == AssignmentActivityStatus.Completed)))
                return AssignmentActivityStatus.Completed;
            return records.Count > 0 ? AssignmentActivityStatus.InProgress : null;
        }

        var statuses = required
            .Select(x => x.ActivityType)
            .Distinct()
            .ToDictionary(type => type, Aggregate);

        return new AssignmentProgressSnapshot
        {
            Status = progress?.Status ?? AssignmentCompletionStatus.NotStarted,
            CompletedActivityCount = progress?.CompletedActivityCount ?? 0,
            RequiredActivityCount = required.Count,
            BestQuizScore = progress?.BestQuizScore,
            CompletedAt = progress?.CompletedAt,
            IsCompletedLate = progress?.IsCompletedLate ?? false,
            ActivityStatuses = statuses
        };
    }
}
