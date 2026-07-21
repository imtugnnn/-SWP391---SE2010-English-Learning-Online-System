using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IUserService
{
    Task<UserServiceResult<List<User>>> GetAllAsync();
    Task<UserServiceResult<User>> GetByIdAsync(int id);

    Task<UserServiceResult<int>> CreateAsync(UserCreateViewModel vm);
    Task<UserServiceResult<object>> UpdateAsync(UserEditViewModel vm);
    Task<UserServiceResult<object>> DeleteAsync(int id);
    Task<UserServiceResult<UserManagementViewModel>> GetUserManagementDataAsync();
}