using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly IClassRepository _classRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStudentManagementService _studentManagementService;

    public TeacherDashboardService(
        IClassRepository classRepository,
        IAssignmentRepository assignmentRepository,
        IStudentManagementService studentManagementService)
    {
        _classRepository = classRepository;
        _assignmentRepository = assignmentRepository;
        _studentManagementService = studentManagementService;
    }

    public async Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(int teacherId)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);

        var courseIds = classes
            .Where(c => c.CourseId.HasValue)
            .Select(c => c.CourseId!.Value)
            .Distinct()
            .ToList();

        var assignments = await _assignmentRepository.GetAssignmentsByCourseIdsAsync(courseIds);

        var totalStudents = 0;
        var classItems = new List<TeacherDashboardClassViewModel>();

        foreach (var classEntity in classes)
        {
            var enrollments = await _classRepository.GetStudentsByClassIdAsync(classEntity.ClassId);

            var classAssignments = assignments
                .Where(a => a.CourseId == classEntity.CourseId)
                .ToList();

            totalStudents += enrollments.Count;

            var expiredAssignmentCount = classAssignments.Count(a => a.DueDate < DateTime.UtcNow);

            classItems.Add(new TeacherDashboardClassViewModel
            {
                ClassId = classEntity.ClassId,
                ClassName = classEntity.ClassName,
                GradeLevel = classEntity.GradeLevel ?? "Chưa cập nhật",
                AcademicYear = classEntity.AcademicYear?.YearLabel ?? "Chưa cập nhật",
                StudentCount = enrollments.Count,
                AssignmentCount = classAssignments.Count,
                ExpiredAssignmentCount = expiredAssignmentCount
            });
        }

        return new TeacherDashboardViewModel
        {
            TeacherName = classes.FirstOrDefault()?.Teacher?.Username ?? "Giáo viên",

            TotalClasses = classes.Count,
            TotalStudents = totalStudents,
            TotalAssignments = assignments.Count,
            ActiveAssignments = assignments.Count(a => a.DueDate >= DateTime.UtcNow),
            ExpiredAssignments = assignments.Count(a => a.DueDate < DateTime.UtcNow),
            StudentsNeedAttention = await _studentManagementService.CountStudentsNeedSupportAsync(teacherId),

            Classes = classItems
        };
    }
}
