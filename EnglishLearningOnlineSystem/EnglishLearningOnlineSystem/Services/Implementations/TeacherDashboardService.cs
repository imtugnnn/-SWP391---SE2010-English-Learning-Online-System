using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class TeacherDashboardService : ITeacherDashboardService
{
    private readonly IClassRepository _classRepository;
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly ITeacherDashboardRepository _teacherDashboardRepository;
    private readonly INotificationRepository _notificationRepository;
    private readonly IStudentManagementService _studentManagementService;

    public TeacherDashboardService(
        IClassRepository classRepository,
        IAssignmentRepository assignmentRepository,
        IStudentManagementService studentManagementService,
        ITeacherDashboardRepository teacherDashboardRepository,
        INotificationRepository notificationRepository)
    {
        _classRepository = classRepository;
        _assignmentRepository = assignmentRepository;
        _studentManagementService = studentManagementService;
        _teacherDashboardRepository = teacherDashboardRepository;
        _notificationRepository = notificationRepository;
    }

    /// <summary>
    /// Tổng hợp dữ liệu dashboard cho giáo viên từ các lớp phụ trách, bài giao và thông báo.
    /// </summary>
    public async Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(int teacherId)
    {
        var classes = await _classRepository.GetClassesByTeacherIdAsync(teacherId);
        // Luồng 1: Repository nạp dữ liệu thô thuộc phạm vi dashboard giáo viên.
        var currentAcademicYear = await _teacherDashboardRepository.GetActiveAcademicYearLabelAsync();

        var classIds = classes
            .Select(c => c.ClassId)
            .Distinct()
            .ToList();

        var assignments = await _assignmentRepository.GetAssignmentsByClassIdsAsync(classIds);
        var publishedNotifications =
            await _teacherDashboardRepository.GetSystemNotificationsByStatusAsync("Đã phát hành");
        var systemNotifications = publishedNotifications
            .Where(notification =>
                notification.UserType == "Tất cả" ||
                notification.UserType == "Giáo viên" ||
                notification.Recipient == "Tất cả người dùng" ||
                notification.Recipient == "Giáo viên")
            .ToList();
        var personalNotifications = await _notificationRepository.GetByUserIdAsync(teacherId);

        var totalStudents = 0;
        var classItems = new List<TeacherDashboardClassViewModel>();

        // Luồng 2: Service tổng hợp thống kê và áp dụng quy tắc quá hạn cho từng lớp.
        foreach (var classEntity in classes)
        {
            // Tính thống kê riêng từng lớp để hiển thị trong danh sách lớp của dashboard.
            var enrollments = await _classRepository.GetActiveStudentsByClassIdAsync(classEntity.ClassId);

            var classAssignments = assignments
                .Where(a => a.ClassId == classEntity.ClassId && a.IsVisible)
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

        // Luồng 3: Ánh xạ Entity sang ViewModel để View chỉ hiển thị dữ liệu.
        return new TeacherDashboardViewModel
        {
            TeacherName = classes.FirstOrDefault()?.Teacher?.Username ?? "Giáo viên",
            CurrentAcademicYear = currentAcademicYear ?? "Chưa cập nhật",

            TotalClasses = classes.Count,
            TotalStudents = totalStudents,
            TotalAssignments = assignments.Count(a => a.IsVisible),
            ActiveAssignments = assignments.Count(a => a.IsVisible && a.DueDate >= DateTime.UtcNow),
            ExpiredAssignments = assignments.Count(a => a.IsVisible && a.DueDate < DateTime.UtcNow),
            StudentsNeedAttention = await _studentManagementService.CountStudentsNeedSupportAsync(teacherId),
            SystemNotifications = systemNotifications.Select(notification =>
                new TeacherSystemNotificationViewModel
                {
                    Title = notification.Title,
                    Content = notification.Content,
                    DisplayTime = notification.PublishTime ?? notification.CreatedAt
                }).ToList(),
            PersonalNotifications = personalNotifications.Select(notification =>
                new TeacherPersonalNotificationViewModel
                {
                    Type = notification.Type,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    DisplayTime = notification.CreateAt.AddHours(7)
                }).ToList(),
            NotificationCount = systemNotifications.Count + personalNotifications.Count(n => !n.IsRead),

            Classes = classItems
        };
    }
}
