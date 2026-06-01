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
            return AuthServiceResult.Failure((nameof(model.Email), "Email is not registered."));
        }

        if (!user.IsActive)
        {
            return AuthServiceResult.Failure((string.Empty, "Your account is inactive. Please contact support."));
        }

        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            return AuthServiceResult.Failure((nameof(model.Password), "Password is incorrect."));
        }

        return AuthServiceResult.Success();
    }

    public async Task<AuthServiceResult> RegisterAsync(RegisterViewModel model)
    {
        var username = model.Username.Trim();
        var email = model.Email.Trim();
        var errors = new List<(string Field, string Message)>();

        if (await _roleRepository.FindRegistrationRoleAsync(model.RoleId) == null)
        {
            errors.Add((nameof(model.RoleId), "Please choose Student or Parent."));
        }

        if (await _userRepository.UsernameExistsAsync(username))
        {
            errors.Add((nameof(model.Username), "Username is already taken."));
        }

        if (await _userRepository.EmailExistsAsync(email))
        {
            errors.Add((nameof(model.Email), "Email is already registered."));
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

        return AuthServiceResult.Success();
    }

    public Task<List<Role>> GetRegistrationRolesAsync()
    {
        return _roleRepository.GetRegistrationRolesAsync();
    }
}
