using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly IClassRepository _classRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IStudentManagementService _studentManagementService;
    private readonly AppDbContext _context;

    public TeacherDashboardService(
        IClassRepository classRepository,
        IAssignmentRepository assignmentRepository,
        IStudentManagementService studentManagementService,
        AppDbContext context)
    {
        _classRepository = classRepository;
        _assignmentRepository = assignmentRepository;
        _studentManagementService = studentManagementService;
        _context = context;
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
        var systemNotifications = await _context.SystemNotifications!
            .Where(n => n.Status == "Đã phát hành" &&
                        (n.UserType == "Tất cả" || n.UserType == "Giáo viên" ||
                         n.Recipient == "Tất cả người dùng" || n.Recipient == "Giáo viên"))
            .OrderByDescending(n => n.PublishTime ?? n.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
        var personalNotifications = await _context.Notifications!
            .Where(n => n.UserId == teacherId)
            .OrderByDescending(n => n.CreateAt)
            .AsNoTracking()
            .ToListAsync();

        var totalStudents = 0;
        var classItems = new List<TeacherDashboardClassViewModel>();

        foreach (var classEntity in classes)
        {
            var enrollments = await _classRepository.GetStudentsByClassIdAsync(classEntity.ClassId);

            var classAssignments = assignments
                .Where(a => a.CourseId == classEntity.CourseId && a.IsVisible)
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
            TotalAssignments = assignments.Count(a => a.IsVisible),
            ActiveAssignments = assignments.Count(a => a.IsVisible && a.DueDate >= DateTime.UtcNow),
            ExpiredAssignments = assignments.Count(a => a.IsVisible && a.DueDate < DateTime.UtcNow),
            StudentsNeedAttention = await _studentManagementService.CountStudentsNeedSupportAsync(teacherId),
            SystemNotifications = systemNotifications,
            PersonalNotifications = personalNotifications,
            NotificationCount = systemNotifications.Count + personalNotifications.Count(n => !n.IsRead),

            Classes = classItems
        };
    }
}
