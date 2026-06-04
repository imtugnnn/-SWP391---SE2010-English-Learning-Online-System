using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

// Interface xử lý nghiệp vụ liên quan đến danh sách khóa học của học sinh
public interface IStudentCourseService
{
    Task<CourseListViewModel> GetCourseListAsync(int studentId, string keyword, string grade);
}