using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class ClassService : IClassService
{
    private readonly IClassRepository _classRepository;

    public ClassService(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    /// <summary>
    /// Lấy thông tin lớp, học sinh, bài giao và tỷ lệ hoàn thành cho giáo viên phụ trách.
    /// </summary>
    public async Task<TeacherClassDetailViewModel?> GetTeacherClassDetailAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetClassDetailByIdAsync(classId);

        if (classEntity == null)
        {
            return null;
        }

        if (classEntity.TeacherId != teacherId)
        {
            // Không tiết lộ dữ liệu lớp cho giáo viên không được phân công.
            return null;
        }

        var enrollments = await _classRepository.GetActiveStudentsByClassIdAsync(classId);
        var assignments = await _classRepository.GetAssignmentsByClassCourseAsync(classEntity.CourseId);

        var studentIds = enrollments.Select(e => e.StudentId).ToList();
        var lessonIds = assignments
            .Where(a => a.LessonId.HasValue)
            .Select(a => a.LessonId!.Value)
            .Distinct()
            .ToList();

        var progressRecords = await _classRepository.GetProgressByStudentIdsAndLessonIdsAsync(studentIds, lessonIds);

        var totalExpectedProgress = studentIds.Count * lessonIds.Count;

        // Tỷ lệ hoàn thành được tính trên tổng số bài mà tất cả học sinh cần hoàn thành.
        var completedProgressCount = progressRecords.Count(p =>
            string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase));

        var completionRate = totalExpectedProgress == 0
            ? 0
            : Math.Round((double)completedProgressCount / totalExpectedProgress * 100, 2);

        var overdueLessonIds = assignments
            .Where(a => a.DueDate < DateTime.UtcNow && a.LessonId.HasValue)
            .Select(a => a.LessonId!.Value)
            .Distinct()
            .ToList();

        // Một học sinh bị trễ tiến độ khi còn ít nhất một bài quá hạn chưa hoàn thành.
        var studentsBehindSchedule = studentIds.Count(studentId =>
            overdueLessonIds.Any(lessonId =>
                !progressRecords.Any(p =>
                    p.StudentId == studentId &&
                    p.LessonId == lessonId &&
                    string.Equals(p.CompletionStatus, "Completed", StringComparison.OrdinalIgnoreCase))));

        return new TeacherClassDetailViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,
            GradeLevel = classEntity.GradeLevel ?? "Chưa cập nhật",
            AcademicYear = classEntity.AcademicYear?.YearLabel ?? "Chưa cập nhật",
            TeacherName = classEntity.Teacher?.Username ?? "Chưa cập nhật",

            StudentCount = enrollments.Count,
            AssignmentCount = assignments.Count,
            CompletionRate = completionRate,
            StudentsBehindSchedule = studentsBehindSchedule,

            Students = enrollments.Select(e => new TeacherClassStudentViewModel
            {
                StudentId = e.StudentId,
                StudentName = e.Student.Username,
                Email = e.Student.Email,
                EnrollmentStatus = "Đang học"
            }).ToList(),

            Assignments = assignments.Select(a => new TeacherClassAssignmentViewModel
            {
                AssignmentId = a.AssignmentId,
                AssignmentTitle = a.Lesson?.Title ?? "Bài học chưa xác định",
                LessonTitle = a.Lesson?.Title ?? "Bài học chưa xác định",
                StartDate = a.WeekStartDate,
                DueDate = a.DueDate,
                Status = GetAssignmentStatus(a.DueDate)
            }).ToList()
        };
    }

    /// <summary>
    /// Chuyển hạn nộp thành trạng thái ngắn gọn để hiển thị cho giáo viên.
    /// </summary>
    private static string GetAssignmentStatus(DateTime dueDate)
    {
        return dueDate < DateTime.UtcNow ? "Quá hạn" : "Đang hoạt động";
    }
}
