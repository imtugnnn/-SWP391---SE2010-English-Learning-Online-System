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

        if (user == null)
        {
            return AuthServiceResult.Failure((nameof(model.Email), "Email chưa được đăng ký."));
        }

        if (!user.IsActive)
        {
            return AuthServiceResult.Failure((string.Empty, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên."));
        }

        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            return AuthServiceResult.Failure((nameof(model.Password), "Mật khẩu không chính xác."));
        }

        return AuthServiceResult.Success();
    }

    public async Task<(AuthServiceResult Result, User? User)> LoginWithGoogleAsync(string email, string? displayName, string? avatarUrl)
    {
        var normalizedEmail = email.Trim().ToLower();
        var user = await _userRepository.FindByEmailAsync(normalizedEmail);

        if (user != null)
        {
            if (!user.IsActive)
            {
                return (AuthServiceResult.Failure((string.Empty, "Tài khoản của bạn đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên.")), null);
            }

            await EnsureStudentProfileAsync(user, displayName, avatarUrl);
            return (AuthServiceResult.Success(), user);
        }

        return (AuthServiceResult.Failure((string.Empty, "Vui lòng hoàn tất thông tin tài khoản để tiếp tục.")), null);
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
            RoleId = model.RoleId
        };

        await _userRepository.AddAsync(user);
        await EnsureStudentProfileAsync(user, string.IsNullOrWhiteSpace(displayName) ? username : displayName, avatarUrl);

        return (AuthServiceResult.Success(), user);
    }

    public async Task<AuthServiceResult> RegisterAsync(RegisterViewModel model)
    {
        var username = model.Username.Trim();
        var email = model.Email.Trim();
        var errors = new List<(string Field, string Message)>();

        if (await _roleRepository.FindRegistrationRoleAsync(model.RoleId) == null)
        {
            errors.Add((nameof(model.RoleId), "Vui lòng chọn vai trò."));
        }

        if (await _userRepository.UsernameExistsAsync(username))
        {
            errors.Add((nameof(model.Username), "Tên đăng nhập đã tồn tại."));
        }

        if (await _userRepository.EmailExistsAsync(email))
        {
            errors.Add((nameof(model.Email), "Email đã được đăng ký."));
        }

        if (errors.Count > 0)
        {
            return AuthServiceResult.Failure(errors.ToArray());
        }

        var user = new User
        {
            Username = username,
            Email = email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            BirthDate = model.BirthDate,
            IsActive = true,
            RoleId = model.RoleId
        };

        await _userRepository.AddAsync(user);
        await EnsureStudentProfileAsync(user, username, null);

        return AuthServiceResult.Success();
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