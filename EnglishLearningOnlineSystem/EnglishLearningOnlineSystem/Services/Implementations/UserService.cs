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
        // B1: Service gọi repository để lấy toàn bộ user.
        => UserServiceResult<List<User>>.Ok(await _userRepo.GetAllAsync());

    public async Task<UserServiceResult<User>> GetByIdAsync(int id)
    {
        // B1: Tra cứu user theo id ở tầng repository.
        var user = await _userRepo.GetByIdAsync(id);
        return user == null
            ? UserServiceResult<User>.Fail("User not found.")
            : UserServiceResult<User>.Ok(user);
    }

    public async Task<UserServiceResult<int>> CreateAsync(UserCreateViewModel vm)
    {
        // B2: Validate trước, rồi mới xuống repository để tạo user.
        if (vm.RoleId <= 0) return UserServiceResult<int>.Fail("Role is required.");
        if (vm.RoleId == 2) return UserServiceResult<int>.Fail("Không được phép tạo tài khoản có vai trò Admin.");

        var ageValidation = ValidateAge(vm.RoleId, vm.BirthDate);
        if (!ageValidation.isValid) return UserServiceResult<int>.Fail(ageValidation.errorMessage);

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

        // B3: Lưu user xuống database thông qua repository.
        await _userRepo.AddAsync(user);
        return UserServiceResult<int>.Ok(user.Id);
    }

    public async Task<UserServiceResult<object>> UpdateAsync(UserEditViewModel vm)
    {
        // B2: Lấy bản ghi hiện tại rồi kiểm tra rule trước khi cập nhật.
        var existing = await _userRepo.GetByIdAsync(vm.Id);
        if (existing == null) return UserServiceResult<object>.Fail("User not found.");

        if (existing.RoleId == 2 || vm.RoleId == 2)
            return UserServiceResult<object>.Fail("Không được phép chỉnh sửa hoặc gán vai trò Admin.");

        var ageValidation = ValidateAge(vm.RoleId, vm.BirthDate);
        if (!ageValidation.isValid) return UserServiceResult<object>.Fail(ageValidation.errorMessage);

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

        // B3: Cập nhật thay đổi xuống database.
        await _userRepo.UpdateAsync(existing);
        return UserServiceResult<object>.Ok(null);
    }

    public async Task<UserServiceResult<object>> DeleteAsync(int id)
    {
        // B2: Lấy user hiện tại để kiểm tra có được xóa hay không.
        var existing = await _userRepo.GetByIdAsync(id);
        if (existing == null) return UserServiceResult<object>.Fail("User not found.");

        if (existing.RoleId == 2)
            return UserServiceResult<object>.Fail("Không được phép xóa tài khoản Admin.");

        // B3: Xóa user qua repository.
        await _userRepo.DeleteAsync(existing);
        return UserServiceResult<object>.Ok(null);
    }

    public async Task<UserServiceResult<UserManagementViewModel>> GetUserManagementDataAsync()
    {
        // B1: Gom users + stats để controller render cho trang quản lý user.
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

    private (bool isValid, string errorMessage) ValidateAge(int roleId, DateTime? birthDate)
    {
        // 1: Student, 2: Admin, 3: Teacher, 5: Content Manager
        if (roleId == 1 || roleId == 2 || roleId == 3 || roleId == 5)
        {
            if (!birthDate.HasValue)
            {
                return (false, "Ngày sinh là bắt buộc.");
            }

            var today = DateTime.Today;
            var age = today.Year - birthDate.Value.Year;
            if (birthDate.Value.Date > today.AddYears(-age))
            {
                age--;
            }

            if (roleId == 1)
            {
                if (age <= 6)
                {
                    return (false, "Học sinh phải lớn hơn 6 tuổi.");
                }
            }
            else // Admin (2), Teacher (3), Content Manager (5)
            {
                if (age < 18 || age > 100)
                {
                    return (false, "Giáo viên, Quản lý nội dung, Admin phải từ 18 đến 100 tuổi.");
                }
            }
        }
        return (true, string.Empty);
    }
}
