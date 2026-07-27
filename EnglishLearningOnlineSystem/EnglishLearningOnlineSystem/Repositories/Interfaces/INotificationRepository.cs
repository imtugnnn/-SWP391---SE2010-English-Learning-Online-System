using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface INotificationRepository
{
    Task AddRangeAsync(IEnumerable<Notification> notifications);
    Task<List<Notification>> GetByUserIdAsync(int userId);
}
