using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Services.Implementations;

// Service xử lý nghiệp vụ liên quan đến bài học được giao cho học sinh
public class StudentLessonService : IStudentLessonService
{
    private readonly IStudentLessonRepository _repo;
    private readonly IAssignmentProgressService _assignmentProgressService;

    public StudentLessonService(
        IStudentLessonRepository repo,
        IAssignmentProgressService assignmentProgressService)
    {
        _repo = repo;
        _assignmentProgressService = assignmentProgressService;
    }

    // Lấy danh sách bài học được giao và hỗ trợ lọc theo trạng thái
    public async Task<AssignedLessonListViewModel> GetAssignedLessonsAsync(int studentId, string filterStatus)
    {
        var assignments = await _repo.GetAssignedLessonsAsync(studentId);
        var items = new List<AssignedLessonItem>();

        foreach (var wa in assignments)
        {
            if (wa.Lesson == null) continue;

            // Business process: tiến độ phải lấy theo assignment, không dùng Progress theo Lesson
            // vì cùng một lesson có thể được giao nhiều lần với cấu hình activity khác nhau.
            var snapshot = await _assignmentProgressService.GetSnapshotAsync(wa.AssignmentId, studentId);

            items.Add(new AssignedLessonItem
            {
                AssignmentId = wa.AssignmentId,
                LessonId = wa.LessonId ?? 0,
                Title = wa.Lesson.Title,
                Topic = wa.Lesson.Topic ?? "",
                CourseName = wa.Lesson.Course?.CourseName ?? "",
                XPReward = wa.Lesson.XPReward,
                EstimatedMinutes = wa.Lesson.EstimatedMinutes,
                VocabularyCount = wa.IncludeVocabulary ? wa.Vocabularies.Count : 0,
                QuizCount = wa.IncludeQuiz ? wa.Quizzes.Count : 0,
                MiniGameCount = wa.IncludeMiniGame ? wa.MiniGames.Count : 0,
                WeekStartDate = wa.WeekStartDate,
                DueDate = wa.DueDate,
                CompletionStatus = ToDisplayStatus(snapshot?.Status ?? AssignmentCompletionStatus.NotStarted),
                QuizScore = snapshot?.BestQuizScore ?? 0,
                FlashcardStatus = ToActivityStatus(snapshot, AssignmentActivityType.Flashcard),
                QuizStatus = ToActivityStatus(snapshot, AssignmentActivityType.Quiz),
                MiniGameStatus = ToActivityStatus(snapshot, AssignmentActivityType.MiniGame),
                CompletedActivityCount = snapshot?.CompletedActivityCount ?? 0,
                RequiredActivityCount = snapshot?.RequiredActivityCount ?? 0,
                IsCompletedLate = snapshot?.IsCompletedLate ?? false
            });
        }

        var normalizedStatus = (filterStatus ?? string.Empty)
            .Trim()
            .ToUpperInvariant() switch
        {
            "" or "ALL" => "",
            "NOT_STARTED" => "NOT_STARTED",
            "IN PROGRESS" => "In Progress",
            "COMPLETED" => "Completed",
            "COMPLETED_LATE" => "COMPLETED_LATE",
            "OVERDUE" => "OVERDUE",
            _ => ""
        };
        var allItems = items;

        // Lọc danh sách theo trạng thái hoàn thành hoặc quá hạn.
        items = normalizedStatus switch
        {
            "" => allItems,
            "OVERDUE" => allItems.Where(i => i.IsOverdue).ToList(),
            "NOT_STARTED" => allItems
                .Where(i => i.CompletionStatus == "NOT_STARTED" && !i.IsOverdue)
                .ToList(),
            "In Progress" => allItems
                .Where(i => i.CompletionStatus == "In Progress" && !i.IsOverdue)
                .ToList(),
            "Completed" => allItems
                .Where(i => i.CompletionStatus == "Completed" && !i.IsCompletedLate)
                .ToList(),
            "COMPLETED_LATE" => allItems.Where(i => i.IsCompletedLate).ToList(),
            _ => allItems
        };

        return new AssignedLessonListViewModel
        {
            Lessons = items,
            FilterStatus = normalizedStatus,
            TotalCount = allItems.Count,
            CompletedCount = allItems.Count(i => i.CompletionStatus == "Completed"),
            InProgressCount = allItems.Count(i =>
                i.CompletionStatus == "In Progress" && !i.IsOverdue),
            NotStartedCount = allItems.Count(i =>
                i.CompletionStatus == "NOT_STARTED" && !i.IsOverdue),
            OverdueCount = allItems.Count(i => i.IsOverdue),
            CompletedLateCount = allItems.Count(i => i.IsCompletedLate)
        };
    }

    private static string ToActivityStatus(
        AssignmentProgressSnapshot? snapshot,
        AssignmentActivityType type)
    {
        if (snapshot == null || !snapshot.ActivityStatuses.TryGetValue(type, out var status))
            return "NotAssigned";
        return status?.ToString() ?? "NotStarted";
    }

    private static string ToDisplayStatus(AssignmentCompletionStatus status) => status switch
    {
        AssignmentCompletionStatus.NotStarted => "NOT_STARTED",
        AssignmentCompletionStatus.InProgress => "In Progress",
        AssignmentCompletionStatus.Completed => "Completed",
        _ => "NOT_STARTED"
    };
}
