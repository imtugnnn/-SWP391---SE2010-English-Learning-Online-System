using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IStudentCommunicationRepository
{
    Task<List<Notification>> GetNotificationsAsync(int studentId);
    Task<Notification?> GetNotificationAsync(int notificationId, int studentId);
    Task<List<TeacherFeedback>> GetFeedbacksAsync(int studentId);
    Task<TeacherFeedback?> GetFeedbackAsync(int feedbackId, int studentId);
    Task<List<Notification>> GetUnreadNotificationsAsync(int studentId, int? feedbackId = null);
    Task SaveChangesAsync();
}
