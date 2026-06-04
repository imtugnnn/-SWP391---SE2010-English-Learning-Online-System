using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers;

public class AdminController : Controller
{
    [HttpGet("/admin/dashboard")]
    public IActionResult Dashboard()
    {
        return View("AdminDashboard");
    }
}
