using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

// Interface xử lý nghiệp vụ liên quan đến bài học của học sinh
public interface IStudentLessonService
{
    Task<AssignedLessonListViewModel> GetAssignedLessonsAsync(int studentId, string filterStatus);
}