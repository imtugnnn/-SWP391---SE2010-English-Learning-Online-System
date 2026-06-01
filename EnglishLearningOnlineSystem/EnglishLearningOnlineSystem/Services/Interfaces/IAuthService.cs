using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IAuthService
{
    Task<AuthServiceResult> LoginAsync(LoginViewModel model);
    Task<AuthServiceResult> RegisterAsync(RegisterViewModel model);
    Task<List<Role>> GetRegistrationRolesAsync();
}
