using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

// Interface xử lý dữ liệu liên quan đến khóa học của học sinh
public interface IStudentCourseRepository
{
    Task<List<Course>> GetAllPublishedAsync(string keyword, string grade);

    Task<List<string>> GetAllGradesAsync();

    Task<List<int>> GetEnrolledCourseIdsAsync(int studentId);

    Task<int> GetLessonCountAsync(int courseId);
}