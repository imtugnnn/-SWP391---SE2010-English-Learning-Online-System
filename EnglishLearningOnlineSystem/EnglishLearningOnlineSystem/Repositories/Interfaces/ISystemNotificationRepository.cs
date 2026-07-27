//Created by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface ISystemNotificationRepository
{
    Task<List<SystemNotification>> GetAllAsync();
    Task<SystemNotification?> GetByIdAsync(int id);
    Task<List<SystemNotification>> GetPublishedVisibleAsync(DateTime now);
    Task<int> PromoteDueScheduledAsync(DateTime now);
    Task AddAsync(SystemNotification notification);
    Task UpdateAsync(SystemNotification notification);
}
