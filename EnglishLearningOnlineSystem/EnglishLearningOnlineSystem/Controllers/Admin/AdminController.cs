using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Admin;

public class AdminController : Controller
{
    public IActionResult Dashboard()
    {
        return View("AdminDashboard");
    }
    
    public IActionResult UserManagement()
    {
        return View("~/Views/Admin/UserManagement/Index.cshtml");
    }
}   
