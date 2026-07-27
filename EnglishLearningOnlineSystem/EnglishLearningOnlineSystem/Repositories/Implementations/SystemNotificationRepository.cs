//Create by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class SystemNotificationRepository : ISystemNotificationRepository
{
    private readonly AppDbContext _context;

    public SystemNotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<SystemNotification>> GetAllAsync()
    {
        return _context.SystemNotifications!
            .Include(n => n.User)
            .AsNoTracking()
            .OrderByDescending(n => n.PublishTime ?? n.CreatedAt)
            .ToListAsync();
    }

    public Task<SystemNotification?> GetByIdAsync(int id)
    {
        return _context.SystemNotifications!
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public Task<List<SystemNotification>> GetPublishedVisibleAsync(DateTime now)
    {
        return _context.SystemNotifications!
            .AsNoTracking()
            .Where(n => n.Status == "Đã phát hành" && (n.PublishTime == null || n.PublishTime <= now))
            .OrderByDescending(n => n.PublishTime ?? n.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> PromoteDueScheduledAsync(DateTime now)
    {
        var dueNotifications = await _context.SystemNotifications!
            .Where(n => n.Status == "Đã lên lịch" && n.PublishTime.HasValue && n.PublishTime <= now)
            .ToListAsync();

        if (dueNotifications.Count == 0)
        {
            return 0;
        }

        foreach (var notification in dueNotifications)
        {
            notification.Status = "Đã phát hành";
            notification.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return dueNotifications.Count;
    }

    public async Task AddAsync(SystemNotification notification)
    {
        _context.SystemNotifications!.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(SystemNotification notification)
    {
        _context.SystemNotifications!.Update(notification);
        await _context.SaveChangesAsync();
    }
}
