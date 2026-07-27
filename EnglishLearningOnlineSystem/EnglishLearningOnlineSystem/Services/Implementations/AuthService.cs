using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;

    public AuthService(IUserRepository userRepository, IRoleRepository roleRepository)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
    }

    public async Task<AuthServiceResult> LoginAsync(LoginViewModel model)
    {
        var email = model.Email.Trim();
        var user = await _userRepository.FindByEmailAsync(email);

        //Kiểm tra nếu tài khoản chưa được đăng ký
        if (user == null)
        {
            return AuthServiceResult.Failure((nameof(model.Email), "Email chưa được đăng ký."));
        }

        // BR-19: Inactive accounts cannot log in.
        if (!user.IsActive)
        {
            return AuthServiceResult.Failure((string.Empty, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên."));
        }
        
        //Kiểm tra nếu tài khoản dang bị khóa do nhập sai 5 lần
        if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.UtcNow)
        {
            var remainingMinutes = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            return AuthServiceResult.Failure((string.Empty, $"Tài khoản của bạn đang tạm thời bị khóa do đăng nhập sai nhiều lần. Vui lòng thử lại sau {remainingMinutes} phút."));
        }

        //Kiểm tra password
        bool isPasswordValid = false;
        try
        {
            isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);
        }
        catch
        {
            isPasswordValid = false;
        }

        //Nếu tài khoản nhập sai mật khẩu, lưu 1 lần thử, sai 5 lần khóa 30p
        if (!isPasswordValid)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
                await _userRepository.UpdateAsync(user);
                return AuthServiceResult.Failure((nameof(model.Password), "Mật khẩu không chính xác. Tài khoản đã bị khóa 30 phút vì đăng nhập sai 5 lần."));
            }
            else
            {
                await _userRepository.UpdateAsync(user);
                return AuthServiceResult.Failure((nameof(model.Password), $"Mật khẩu không chính xác. Bạn còn {5 - user.AccessFailedCount} lần thử."));
            }
        }

        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return AuthServiceResult.Success();
    }

    public async Task<(AuthServiceResult Result, User? User)> LoginWithGoogleAsync(string email, string? displayName, string? avatarUrl)
    {
        var normalizedEmail = email.Trim().ToLower();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail);

        if (user != null)
        {
            // BR-19: Inactive accounts cannot log in.
            if (!user.IsActive)
            {
                return (AuthServiceResult.Failure((string.Empty, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.")), null);
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await EnsureStudentProfileAsync(user, displayName, avatarUrl);
            return (AuthServiceResult.Success(), user);
        }

        return (AuthServiceResult.Failure((string.Empty, "Tài khoản Google này chưa được đăng ký trong hệ thống.")), null);
    }

    public async Task<(AuthServiceResult Result, User? User)> CompleteGoogleLoginAsync(GoogleLoginCompletionViewModel model, string? displayName, string? avatarUrl)
    {
        var username = model.Username.Trim();
        var email = model.Email.Trim().ToLower();
        var errors = new List<(string Field, string Message)>();

        if (await _roleRepository.FindRegistrationRoleAsync(model.RoleId) == null)
        {
            errors.Add((nameof(model.RoleId), "Vui lòng chọn vai trò."));
        }

        if (await _userRepository.UsernameExistsAsync(username))
        {
            errors.Add((nameof(model.Username), "Tên đăng nhập đã tồn tại."));
        }

        // BR-20: Email addresses must be unique.
        if (await _userRepository.EmailExistsAsync(email))
        {
            errors.Add((nameof(model.Email), "Email đã được đăng ký. Vui lòng sử dụng chức năng Đăng nhập bằng Google."));
        }

        if (errors.Count > 0)
        {
            return (AuthServiceResult.Failure(errors.ToArray()), null);
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            BirthDate = model.BirthDate,
            IsActive = true,
            RoleId = model.RoleId,
            LastLoginAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await EnsureStudentProfileAsync(user, string.IsNullOrWhiteSpace(displayName) ? username : displayName, avatarUrl);

        return (AuthServiceResult.Success(), user);
    }

    public Task<List<Role>> GetRegistrationRolesAsync()
    {
        return _roleRepository.GetRegistrationRolesAsync();
    }

    private async Task EnsureStudentProfileAsync(User user, string? displayName, string? avatarUrl)
    {
        if (user.RoleId != 1 || await _userRepository.FindStudentProfileAsync(user.Id) != null)
        {
            return;
        }

        var trimmedAvatarUrl = avatarUrl?.Trim();

        await _userRepository.AddStudentProfileAsync(new StudentProfile
        {
            StudentId = user.Id,
            Nickname = string.IsNullOrWhiteSpace(displayName) ? user.Username : displayName.Trim(),
            AvatarUrl = string.IsNullOrWhiteSpace(trimmedAvatarUrl) ? "/images/default-avatar.png" : trimmedAvatarUrl[..Math.Min(trimmedAvatarUrl.Length, 500)],
            Level = 1,
            XP = 0,
            CurrentStreakDays = 0,
            LastActiveDate = null
        });
    }
}