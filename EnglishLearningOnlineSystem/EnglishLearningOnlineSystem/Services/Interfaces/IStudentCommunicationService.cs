using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IStudentCommunicationService
{
    Task<StudentNotificationCenterViewModel> GetNotificationsAsync(int studentId, string? filter);
    Task<bool> MarkNotificationReadAsync(int notificationId, int studentId);
    Task MarkAllNotificationsReadAsync(int studentId);
    Task<StudentTeacherFeedbackViewModel> GetTeacherFeedbackAsync(int studentId);
    Task<bool> MarkFeedbackReadAsync(int feedbackId, int studentId);
}
