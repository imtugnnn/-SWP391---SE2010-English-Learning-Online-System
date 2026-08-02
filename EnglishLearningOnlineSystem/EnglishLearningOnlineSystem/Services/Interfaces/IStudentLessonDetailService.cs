using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

// Interface xử lý nghiệp vụ chi tiết bài học và kết quả làm bài
public interface IStudentLessonDetailService
{
    Task<LessonDetailViewModel?> GetLessonDetailAsync(
        int studentId,
        int lessonId,
        int? assignmentId = null);

    Task<(bool ok, string message)> SubmitQuizAsync(
        int studentId,
        int lessonId,
        Dictionary<int, string> answers);
}
