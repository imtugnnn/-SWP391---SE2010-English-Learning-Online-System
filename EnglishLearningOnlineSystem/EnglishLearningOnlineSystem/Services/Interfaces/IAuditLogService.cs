using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task LogActivityAsync(int userId, string action);
    }
}
