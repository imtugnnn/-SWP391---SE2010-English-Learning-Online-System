using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.ViewModels.Admin;

public class SystemNotificationIndexViewModel
{
    public List<SystemNotification> Notifications { get; set; } = new();
    public List<Role> Roles { get; set; } = new();
}
