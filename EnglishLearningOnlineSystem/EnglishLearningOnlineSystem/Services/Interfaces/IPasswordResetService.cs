using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IPasswordResetService
{
    Task RequestPasswordResetAsync(ForgotPasswordViewModel model, Func<string, string> resetUrlFactory);
    Task<AuthServiceResult> ResetPasswordAsync(ResetPasswordViewModel model);
}
