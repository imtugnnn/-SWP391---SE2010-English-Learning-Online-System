using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

// Service xử lý nghiệp vụ liên quan đến bài học được giao cho học sinh
public class StudentLessonService : IStudentLessonService
{
    private readonly IStudentLessonRepository _repo;

    public StudentLessonService(IStudentLessonRepository repo)
    {
        _repo = repo;
    }

    // Lấy danh sách bài học được giao và hỗ trợ lọc theo trạng thái
    public async Task<AssignedLessonListViewModel> GetAssignedLessonsAsync(int studentId, string filterStatus)
    {
        var assignments = await _repo.GetAssignedLessonsAsync(studentId);
        var items = new List<AssignedLessonItem>();

        foreach (var wa in assignments)
        {
            if (wa.Lesson == null) continue;

            // Lấy tiến độ tốt nhất của học sinh cho bài học
            var progress = await _repo.GetBestProgressAsync(studentId, wa.LessonId ?? 0);

            items.Add(new AssignedLessonItem
            {
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
                CompletionStatus = progress?.CompletionStatus ?? "NOT_STARTED",
                QuizScore = progress?.QuizScore ?? 0
            });
        }

        // Lọc danh sách theo trạng thái hoàn thành
        if (!string.IsNullOrEmpty(filterStatus) && filterStatus != "ALL")
            items = items.Where(i => i.CompletionStatus == filterStatus).ToList();

        return new AssignedLessonListViewModel
        {
            Lessons = items,
            FilterStatus = filterStatus
        };
    }
}
