using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

// Service xử lý logic nghiệp vụ liên quan đến danh sách khóa học của học sinh
public class StudentCourseService : IStudentCourseService
{
    private readonly IStudentCourseRepository _repo;
    private readonly IStudentLessonRepository _lessonRepo;

    public StudentCourseService(
        IStudentCourseRepository repo,
        IStudentLessonRepository lessonRepo)
    {
        _repo = repo;
        _lessonRepo = lessonRepo;
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

    // Lấy chi tiết một khóa học kèm danh sách bài học và tiến độ học sinh
    public async Task<CourseDetailViewModel?> GetCourseDetailAsync(int studentId, int courseId)
    {
        var course = await _repo.GetCourseWithLessonsAsync(courseId);
        if (course == null) return null;

        var enrolledIds = await _repo.GetEnrolledCourseIdsAsync(studentId);

        var lessonItems = new List<CourseLessonItem>();

        foreach (var lesson in course.Lessons ?? new List<EnglishLearningOnlineSystem.Models.Lesson>())
        {
            // Lấy tiến độ tốt nhất của học sinh cho từng bài học
            var progress = await _lessonRepo.GetBestProgressAsync(studentId, lesson.LessonId);

            lessonItems.Add(new CourseLessonItem
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Topic = lesson.Topic ?? "",
                XPReward = lesson.XPReward,
                EstimatedMinutes = lesson.EstimatedMinutes,
                OrderIndex = lesson.OrderIndex,
                CompletionStatus = progress?.CompletionStatus ?? "NOT_STARTED",
                BestScore = progress?.QuizScore ?? 0
            });
        }

        return new CourseDetailViewModel
        {
            CourseId = course.CourseId,
            CourseName = course.CourseName,
            GradeLevel = course.GradeLevel ?? "",
            IsEnrolled = enrolledIds.Contains(course.CourseId),
            Lessons = lessonItems
        };
    }
}
