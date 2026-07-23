//Create by TungDPL
//Last update: 7/3/2026
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IAuthService
{
    Task<AuthServiceResult> LoginAsync(LoginViewModel model);
    Task<(AuthServiceResult Result, User? User)> LoginWithGoogleAsync(string email, string? displayName, string? avatarUrl);
    Task<(AuthServiceResult Result, User? User)> CompleteGoogleLoginAsync(GoogleLoginCompletionViewModel model, string? displayName, string? avatarUrl);
    Task<List<Role>> GetRegistrationRolesAsync();
}
