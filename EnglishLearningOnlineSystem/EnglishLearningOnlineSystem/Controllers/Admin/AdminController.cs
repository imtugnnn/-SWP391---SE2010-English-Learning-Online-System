using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AdminController : Controller
{
    public IActionResult Dashboard()
    {
        return View("AdminDashboard");
    }
}
