using EnglishLearningOnlineSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers;

public class StudentController : Controller
{
    private readonly IStudentDashboardService _dashboardService;

    public StudentController(IStudentDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("/student/dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return RedirectToAction("Login", "Auth");

        var vm = await _dashboardService.GetDashboardAsync(userId.Value);

        // StudentProfile chưa được tạo → hiển thị trang tạm thay vì redirect login
        if (vm == null)
            return Content($"[Debug] Login OK. UserId = {userId}. StudentProfile chưa có trong DB. Cần seed data hoặc tạo profile.");

        if (vm.IsFirstLogin)
            return RedirectToAction(nameof(Onboarding));

        return View(vm);
    }

    [HttpGet("/student/onboarding")]
    public IActionResult Onboarding()
    {
        return View();
    }

    [HttpPost("/student/onboarding/complete")]
    [ValidateAntiForgeryToken]
    public IActionResult OnboardingComplete()
    {
        return RedirectToAction(nameof(Dashboard));
    }

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        if (int.TryParse(raw, out var id)) return id;
        return null;
    }
}