using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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

        return result.Succeeded
            ? RedirectToAction(nameof(HomeController.Homepage), "Home")
            : ViewWithErrors(model, result);
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
