using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

// Service xử lý logic nghiệp vụ liên quan đến danh sách khóa học của học sinh
public class StudentCourseService : IStudentCourseService
{
    private readonly IStudentCourseRepository _repo;

    public StudentCourseService(IStudentCourseRepository repo)
    {
        _repo = repo;
    }

    // Lấy danh sách khóa học kèm thông tin hiển thị cho giao diện
    public async Task<CourseListViewModel> GetCourseListAsync(int studentId, string keyword, string grade)
    {
        var courses = await _repo.GetAllPublishedAsync(keyword, grade);
        var grades = await _repo.GetAllGradesAsync();
        var enrolledIds = await _repo.GetEnrolledCourseIdsAsync(studentId);

        var summaries = new List<CourseSummary>();

        foreach (var c in courses)
        {
            summaries.Add(new CourseSummary
            {
                CourseId = c.CourseId,
                CourseName = c.CourseName,
                GradeLevel = c.GradeLevel ?? "",
                LessonCount = await _repo.GetLessonCountAsync(c.CourseId),
                IsEnrolled = enrolledIds.Contains(c.CourseId)
            });
        }

        return new CourseListViewModel
        {
            Courses = summaries,
            Keyword = keyword,
            Grade = grade,
            Grades = grades
        };
    }
}