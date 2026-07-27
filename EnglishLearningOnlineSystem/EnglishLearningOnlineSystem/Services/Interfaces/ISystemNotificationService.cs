//Created by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels.Admin;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ISystemNotificationService
{
    Task RefreshDueScheduledAsync();
    Task<SystemNotificationServiceResult<SystemNotificationIndexViewModel>> GetIndexDataAsync();
    Task<SystemNotificationServiceResult<List<EnglishLearningOnlineSystem.Models.SystemNotification>>> GetAdminNotificationsAsync();
    Task<SystemNotificationServiceResult<object>> CreateAsync(SystemNotificationViewModel vm, int currentAdminId);
    Task<SystemNotificationServiceResult<object>> UpdateAsync(SystemNotificationViewModel vm, int? currentAdminId);
    Task<SystemNotificationServiceResult<object>> DeleteAsync(int id, int? currentAdminId);
}
