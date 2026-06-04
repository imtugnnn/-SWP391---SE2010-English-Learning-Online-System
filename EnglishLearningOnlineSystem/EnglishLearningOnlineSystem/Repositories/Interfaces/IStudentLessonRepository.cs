using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

// Interface xử lý dữ liệu liên quan đến bài học của học sinh
public interface IStudentLessonRepository
{
    Task<List<WeeklyAssignment>> GetAssignedLessonsAsync(int studentId);

    Task<Progress?> GetBestProgressAsync(int studentId, int lessonId);
}