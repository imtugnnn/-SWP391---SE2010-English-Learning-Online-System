using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
}