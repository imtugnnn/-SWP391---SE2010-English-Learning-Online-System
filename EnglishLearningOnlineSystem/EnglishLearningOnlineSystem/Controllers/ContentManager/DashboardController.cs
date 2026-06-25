using EnglishLearningOnlineSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

[Route("contentmanager/dashboard")]
[Route("contentmanager")]
public class DashboardController : BaseContentManagerController
{
    public DashboardController(AppDbContext db) : base(db)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var totalCourses = await _db.Courses.CountAsync(c => !c.IsDeleted);
        var totalLessons = await _db.Lessons.CountAsync();
        var totalVocabularies = await _db.Vocabularies.CountAsync();
        var totalQuizzes = await _db.Quizzes.CountAsync();

        ViewBag.TotalCourses = totalCourses;
        ViewBag.TotalLessons = totalLessons;
        ViewBag.TotalVocabularies = totalVocabularies;
        ViewBag.TotalQuizzes = totalQuizzes;

        return View("~/Views/ContentManager/Dashboard/Index.cshtml");
    }
}
