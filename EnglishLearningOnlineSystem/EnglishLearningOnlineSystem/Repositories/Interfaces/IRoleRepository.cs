using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> FindRegistrationRoleAsync(int roleId);
    Task<List<Role>> GetRegistrationRolesAsync();
    Task<List<Role>> GetAllAsync();
}
