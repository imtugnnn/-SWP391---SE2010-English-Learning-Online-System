using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class StudentCommunicationService : IStudentCommunicationService
{
    private readonly IStudentCommunicationRepository _repository;

    public StudentCommunicationService(IStudentCommunicationRepository repository)
    {
        _repository = repository;
    }

    public async Task<StudentNotificationCenterViewModel> GetNotificationsAsync(
        int studentId,
        string? filter)
    {
        var normalizedFilter = NormalizeFilter(filter);
        var notifications = await _repository.GetNotificationsAsync(studentId);
        var unreadCount = notifications.Count(x => !x.IsRead);

        notifications = normalizedFilter switch
        {
            "unread" => notifications.Where(x => !x.IsRead).ToList(),
            "assignment" => notifications.Where(x => x.Type.StartsWith("ASSIGNMENT", StringComparison.OrdinalIgnoreCase) || x.Type == "NEW_ASSIGNMENT").ToList(),
            "feedback" => notifications.Where(x => x.Type == "TEACHER_FEEDBACK").ToList(),
            _ => notifications
        };

        return new StudentNotificationCenterViewModel
        {
            Filter = normalizedFilter,
            UnreadCount = unreadCount,
            Notifications = notifications.Select(x => new StudentNotificationItemViewModel
            {
                NotificationId = x.NotificationId,
                Type = x.Type,
                Title = GetNotificationTitle(x.Type),
                Message = x.Message,
                CreatedAt = x.CreateAt,
                IsRead = x.IsRead,
                TargetUrl = BuildTargetUrl(x)
            }).ToList()
        };
    }

    public async Task<bool> MarkNotificationReadAsync(int notificationId, int studentId)
    {
        var notification = await _repository.GetNotificationAsync(notificationId, studentId);
        if (notification == null) return false;
        notification.IsRead = true;
        await _repository.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllNotificationsReadAsync(int studentId)
    {
        var notifications = await _repository.GetUnreadNotificationsAsync(studentId);
        foreach (var notification in notifications) notification.IsRead = true;
        await _repository.SaveChangesAsync();
    }

    public async Task<StudentTeacherFeedbackViewModel> GetTeacherFeedbackAsync(int studentId)
    {
        var feedbacks = await _repository.GetFeedbacksAsync(studentId);
        return new StudentTeacherFeedbackViewModel
        {
            UnreadCount = feedbacks.Count(x => !x.IsRead),
            Feedbacks = feedbacks.Select(x => new StudentTeacherFeedbackItemViewModel
            {
                FeedbackId = x.FeedbackId,
                TeacherName = x.Teacher?.Username ?? "Giáo viên",
                ClassName = x.Class?.ClassName ?? "Phản hồi học tập",
                AssignmentTitle = x.Assignment?.Lesson?.Title,
                Content = x.Content,
                CreatedAt = x.CreateAt,
                IsRead = x.IsRead
            }).ToList()
        };
    }

    public async Task<bool> MarkFeedbackReadAsync(int feedbackId, int studentId)
    {
        var feedback = await _repository.GetFeedbackAsync(feedbackId, studentId);
        if (feedback == null) return false;

        // Business process: đọc feedback đồng thời đóng notification tương ứng để hai màn hình nhất quán.
        feedback.IsRead = true;
        var notifications = await _repository.GetUnreadNotificationsAsync(studentId, feedbackId);
        foreach (var notification in notifications) notification.IsRead = true;
        await _repository.SaveChangesAsync();
        return true;
    }

    private static string NormalizeFilter(string? filter)
    {
        var value = filter?.Trim().ToLowerInvariant();
        return value is "unread" or "assignment" or "feedback" ? value : "all";
    }

    private static string GetNotificationTitle(string type) => type switch
    {
        "NEW_ASSIGNMENT" => "Bài giao mới",
        "ASSIGNMENT_UPDATED" => "Bài giao đã cập nhật",
        "ASSIGNMENT_CANCELLED" => "Bài giao đã hủy",
        "TEACHER_FEEDBACK" => "Phản hồi từ giáo viên",
        _ => "Thông báo"
    };

    private static string? BuildTargetUrl(Notification notification)
    {
        if (notification.Type == "TEACHER_FEEDBACK") return "/student/feedback";
        if (notification.Assignment?.LessonId != null)
            return $"/student/lesson/{notification.Assignment.LessonId}?assignmentId={notification.AssignmentId}";
        return notification.Type.StartsWith("ASSIGNMENT", StringComparison.OrdinalIgnoreCase) ||
               notification.Type == "NEW_ASSIGNMENT"
            ? "/student/lessons"
            : null;
    }
}
