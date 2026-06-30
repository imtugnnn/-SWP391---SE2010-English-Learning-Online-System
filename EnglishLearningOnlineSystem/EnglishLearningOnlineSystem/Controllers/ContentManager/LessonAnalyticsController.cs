using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

[Route("ContentManager/Analytics")]
public class LessonAnalyticsController : BaseContentManagerController
{
    private readonly ILessonAnalyticsService _analyticsService;

    public LessonAnalyticsController(
        AppDbContext db,
        ILessonAnalyticsService analyticsService) : base(db)
    {
        _analyticsService = analyticsService;
    }

    // GET: /ContentManager/Analytics
    [HttpGet("")]
    public async Task<IActionResult> Index(int? courseId, string? search, string? sortBy)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _analyticsService.GetDashboardAsync(courseId, search, sortBy);
        return View("~/Views/ContentManager/Analytics/Index.cshtml", vm);
    }

    // GET: /ContentManager/Analytics/Details/5
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int id, int days = 30)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _analyticsService.GetDetailAsync(id, days);

        if (vm == null)
            return NotFound();

        return View("~/Views/ContentManager/Analytics/Details.cshtml", vm);
    }

    private bool IsAuthorized()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return role == "1" || role == "5";
    }

    private IActionResult RedirectToLogin()
        => RedirectToAction("Login", "Auth");
}