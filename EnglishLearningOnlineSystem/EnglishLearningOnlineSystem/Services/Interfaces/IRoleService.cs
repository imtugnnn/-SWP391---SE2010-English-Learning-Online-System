//Created by TungDPL
//Last update: 7/21/2026
using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
}