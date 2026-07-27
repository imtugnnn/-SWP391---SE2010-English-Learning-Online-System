using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class TeacherDashboardRepository : ITeacherDashboardRepository
{
    private readonly AppDbContext _context;

    public TeacherDashboardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetActiveAcademicYearLabelAsync()
    {
        return await _context.AcademicYears!
            .Where(year => year.IsActive)
            .Select(year => year.YearLabel)
            .FirstOrDefaultAsync();
    }

    public async Task<List<SystemNotification>> GetSystemNotificationsByStatusAsync(string status)
    {
        return await _context.SystemNotifications!
            .Where(notification => notification.Status == status)
            .OrderByDescending(notification => notification.PublishTime ?? notification.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }
}
