using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Interfaces;

namespace EnglishLearningOnlineSystem.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogActivityAsync(int userId, string action)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
            if (user != null)
            {
                _context.AuditLogs.Add(new AuditLog
                {
                    UserId = userId,
                    Username = user.Username,
                    UserRole = user.Role?.Name ?? "Admin",
                    Action = action,
                    Timestamp = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
            }
        }
    }
}
