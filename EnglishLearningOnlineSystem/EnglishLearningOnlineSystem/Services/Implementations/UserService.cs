using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.Admin;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepo;

    public UserService(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<UserServiceResult<List<User>>> GetAllAsync()
        => UserServiceResult<List<User>>.Ok(await _userRepo.GetAllAsync());

    public async Task<UserServiceResult<User>> GetByIdAsync(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        return user == null
            ? UserServiceResult<User>.Fail("User not found.")
            : UserServiceResult<User>.Ok(user);
    }

    public async Task<UserServiceResult<int>> CreateAsync(UserCreateViewModel vm)
    {
        if (vm.RoleId <= 0) return UserServiceResult<int>.Fail("Role is required.");

        if (await _userRepo.UsernameExistsAsync(vm.Username))
            return UserServiceResult<int>.Fail("Username already exists.");

        // BR-20: Email addresses must be unique.
        if (await _userRepo.EmailExistsAsync(vm.Email))
            return UserServiceResult<int>.Fail("Email already exists.");

        var user = new User
        {
            Username = vm.Username,
            Email = vm.Email,
            Password = vm.Password, // TODO: hash
            BirthDate = vm.BirthDate,
            IsActive = vm.IsActive,
            RoleId = vm.RoleId
        };

        await _userRepo.AddAsync(user);
        return UserServiceResult<int>.Ok(user.Id);
    }

    public async Task<UserServiceResult<object>> UpdateAsync(UserEditViewModel vm)
    {
        var existing = await _userRepo.GetByIdAsync(vm.Id);
        if (existing == null) return UserServiceResult<object>.Fail("User not found.");

        // cách B: chỉ check trùng nếu có thay đổi
        if (!string.Equals(existing.Username, vm.Username, StringComparison.Ordinal)
            && await _userRepo.UsernameExistsAsync(vm.Username))
            return UserServiceResult<object>.Fail("Username already exists.");

        // BR-20: Email addresses must be unique.
        if (!string.Equals(existing.Email, vm.Email, StringComparison.OrdinalIgnoreCase)
            && await _userRepo.EmailExistsAsync(vm.Email))
            return UserServiceResult<object>.Fail("Email already exists.");

        existing.Username = vm.Username;
        existing.Email = vm.Email;
        existing.BirthDate = vm.BirthDate;
        existing.IsActive = vm.IsActive;
        existing.RoleId = vm.RoleId;

        // Password: để trống => giữ nguyên
        if (!string.IsNullOrWhiteSpace(vm.Password))
        {
            existing.Password = vm.Password; // TODO: hash
        }

        await _userRepo.UpdateAsync(existing);
        return UserServiceResult<object>.Ok(null);
    }

    public async Task<UserServiceResult<object>> DeleteAsync(int id)
    {
        var existing = await _userRepo.GetByIdAsync(id);
        if (existing == null) return UserServiceResult<object>.Fail("User not found.");

        await _userRepo.DeleteAsync(existing);
        return UserServiceResult<object>.Ok(null);
    }

    public async Task<UserServiceResult<UserManagementViewModel>> GetUserManagementDataAsync()
    {
        var now = DateTime.Now;
        var thisMonthStart = new DateTime(now.Year, now.Month, 1);
        var lastMonthStart = thisMonthStart.AddMonths(-1);

        var users = await _userRepo.GetAllAsync();
        var stats = await _userRepo.GetUserStatsAsync(thisMonthStart, lastMonthStart);

        var vm = new UserManagementViewModel
        {
            Users = users,
            Stats = stats
        };

        return UserServiceResult<UserManagementViewModel>.Ok(vm);
    }
}