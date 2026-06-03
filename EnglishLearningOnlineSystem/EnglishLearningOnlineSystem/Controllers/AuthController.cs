using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace EnglishLearningOnlineSystem.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly IUserRepository _userRepository;

    public AuthController(IAuthService authService, IUserRepository userRepository)
    {
        _authService = authService;
        _userRepository = userRepository;
    }

    [HttpGet("/login")]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost("/login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.LoginAsync(model);

        if (!result.Succeeded)
        {
            return ViewWithErrors(model, result);
        }

        var user = await _userRepository.FindByEmailAsync(model.Email.Trim());
        if (user != null)
        {
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("UserRole", user.RoleId.ToString());
        }

        // RoleId: 1=Student, 2=Admin, 3=Teacher, 4=Parent, 5=Content Manager
        return user?.RoleId == 1
            ? RedirectToAction(nameof(StudentController.Dashboard), "Student")
            : RedirectToAction(nameof(HomeController.Homepage), "Home");
    }

    [HttpGet("/login/google")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(GoogleCallback), "Auth")
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("/login/google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            ModelState.AddModelError(string.Empty, "Google did not return an email address.");
            return View(nameof(Login), new LoginViewModel());
        }

        var displayName = User.FindFirstValue(ClaimTypes.Name);
        var avatarUrl = User.FindFirstValue("urn:google:picture") ?? User.FindFirstValue("picture");
        var (result, user) = await _authService.LoginWithGoogleAsync(email, displayName, avatarUrl);

        if (!result.Succeeded || user == null)
        {
            return ViewWithErrors(new LoginViewModel { Email = email }, result);
        }

        HttpContext.Session.SetString("UserId", user.Id.ToString());
        HttpContext.Session.SetString("UserRole", user.RoleId.ToString());

        return user.RoleId == 1
            ? RedirectToAction(nameof(StudentController.Dashboard), "Student")
            : RedirectToAction(nameof(HomeController.Homepage), "Home");
    }

    [HttpGet("/register")]
    public async Task<IActionResult> Register()
    {
        return View(new RegisterViewModel
        {
            RoleOptions = await LoadRoleOptionsAsync()
        });
    }

    [HttpPost("/register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.RoleOptions = await LoadRoleOptionsAsync();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _authService.RegisterAsync(model);

        return result.Succeeded
            ? RedirectToAction(nameof(Login))
            : ViewWithErrors(model, result);
    }

    private async Task<List<SelectListItem>> LoadRoleOptionsAsync()
    {
        var roles = await _authService.GetRegistrationRolesAsync();

        return roles
            .Select(role => new SelectListItem
            {
                Value = role.Id.ToString(),
                Text = role.Name
            })
            .ToList();
    }

    private IActionResult ViewWithErrors<TModel>(TModel model, AuthServiceResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(error.Key, error.Value);
        }

        return View(model);
    }
}
