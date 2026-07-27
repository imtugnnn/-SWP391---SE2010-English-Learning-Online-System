//Create by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels.Admin;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class SystemNotificationService : ISystemNotificationService
{
    private readonly ISystemNotificationRepository _notificationRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogService _auditLogService;

    public SystemNotificationService(
        ISystemNotificationRepository notificationRepository,
        IRoleRepository roleRepository,
        IAuditLogService auditLogService)
    {
        _notificationRepository = notificationRepository;
        _roleRepository = roleRepository;
        _auditLogService = auditLogService;
    }

    public async Task RefreshDueScheduledAsync()
    {
        await _notificationRepository.PromoteDueScheduledAsync(DateTime.Now);
    }

    public async Task<SystemNotificationServiceResult<SystemNotificationIndexViewModel>> GetIndexDataAsync()
    {
        await RefreshDueScheduledAsync();
        var notifications = await _notificationRepository.GetAllAsync();
        var roles = await _roleRepository.GetAllAsync();

        return SystemNotificationServiceResult<SystemNotificationIndexViewModel>.Ok(
            new SystemNotificationIndexViewModel
            {
                Notifications = notifications,
                Roles = roles
            });
    }

    public async Task<SystemNotificationServiceResult<List<SystemNotification>>> GetAdminNotificationsAsync()
    {
        await RefreshDueScheduledAsync();
        var now = DateTime.Now;
        var notifications = await _notificationRepository.GetPublishedVisibleAsync(now);

        var adminNotifications = notifications
            .Where(n => n.Recipient != null && (
                n.Recipient.Equals("Tất cả người dùng", StringComparison.OrdinalIgnoreCase) ||
                n.Recipient.Equals("Tất cả", StringComparison.OrdinalIgnoreCase) ||
                n.Recipient.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().ToLower())
                    .Any(s => s == "2" || s == "admin")))
            .ToList();

        return SystemNotificationServiceResult<List<SystemNotification>>.Ok(adminNotifications);
    }

    public async Task<SystemNotificationServiceResult<object>> CreateAsync(SystemNotificationViewModel vm, int currentAdminId)
    {
        if (vm == null)
        {
            return SystemNotificationServiceResult<object>.Fail("Dữ liệu không hợp lệ.");
        }

        var notification = new SystemNotification
        {
            Title = vm.Title,
            Content = vm.Content,
            Recipient = vm.Recipient,
            UserType = vm.UserType,
            Status = vm.Status,
            PublishTime = vm.Status == "Bản nháp" ? null : (vm.PublishTime ?? DateTime.Now),
            UserId = currentAdminId <= 0 ? 1 : currentAdminId,
            CreatedAt = DateTime.Now
        };

        await _notificationRepository.AddAsync(notification);
        await _auditLogService.LogActivityAsync(notification.UserId, $"Tạo thông báo hệ thống: '{notification.Title}' ({notification.Status})");

        return SystemNotificationServiceResult<object>.Ok(null);
    }

    public async Task<SystemNotificationServiceResult<object>> UpdateAsync(SystemNotificationViewModel vm, int? currentAdminId)
    {
        if (vm == null)
        {
            return SystemNotificationServiceResult<object>.Fail("Dữ liệu không hợp lệ.");
        }

        var notification = await _notificationRepository.GetByIdAsync(vm.Id);
        if (notification == null)
        {
            return SystemNotificationServiceResult<object>.Fail("Không tìm thấy thông báo.");
        }

        if (notification.Status == "Đã phát hành")
        {
            return SystemNotificationServiceResult<object>.Fail("Thông báo đã phát hành không thể chỉnh sửa.");
        }

        notification.Title = vm.Title;
        notification.Content = vm.Content;
        notification.Recipient = vm.Recipient;
        notification.UserType = vm.UserType;
        notification.Status = vm.Status;
        notification.PublishTime = vm.Status == "Bản nháp" ? null : (vm.PublishTime ?? DateTime.Now);
        notification.UpdatedAt = DateTime.Now;

        await _notificationRepository.UpdateAsync(notification);

        if (currentAdminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(currentAdminId.Value, $"Chỉnh sửa thông báo hệ thống: '{notification.Title}' (ID: {notification.Id})");
        }

        return SystemNotificationServiceResult<object>.Ok(null);
    }

    public async Task<SystemNotificationServiceResult<object>> DeleteAsync(int id, int? currentAdminId)
    {
        var notification = await _notificationRepository.GetByIdAsync(id);
        if (notification == null)
        {
            return SystemNotificationServiceResult<object>.Fail("Không tìm thấy thông báo.");
        }

        if (notification.Status == "Đã phát hành")
        {
            return SystemNotificationServiceResult<object>.Fail("Không thể hủy thông báo đã phát hành.");
        }

        notification.Status = "Đã hủy";
        notification.UpdatedAt = DateTime.Now;

        await _notificationRepository.UpdateAsync(notification);

        if (currentAdminId.HasValue)
        {
            await _auditLogService.LogActivityAsync(currentAdminId.Value, $"Hủy (xóa mềm) thông báo hệ thống: '{notification.Title}' (ID: {notification.Id})");
        }

        return SystemNotificationServiceResult<object>.Ok(null);
    }
}
