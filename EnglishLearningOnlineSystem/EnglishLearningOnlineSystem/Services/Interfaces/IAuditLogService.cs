using System.Collections.Generic;
using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActivityAsync(int userId, string action);
        Task<List<EnglishLearningOnlineSystem.Models.AuditLog>> GetLatestAsync(int count);
    }
}
