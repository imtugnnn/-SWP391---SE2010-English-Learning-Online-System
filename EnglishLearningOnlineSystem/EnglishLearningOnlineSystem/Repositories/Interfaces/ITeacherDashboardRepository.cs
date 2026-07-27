using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface ITeacherDashboardRepository
{
    Task<string?> GetActiveAcademicYearLabelAsync();
    Task<List<SystemNotification>> GetSystemNotificationsByStatusAsync(string status);
}
