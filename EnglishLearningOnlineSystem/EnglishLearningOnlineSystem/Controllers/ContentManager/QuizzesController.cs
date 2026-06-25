using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Quizzes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

[Route("contentmanager/quizzes")]
public class QuizzesController : BaseContentManagerController
{
    private const int DefaultPageSize = 10;
    private readonly IQuizService _quizService;
    private readonly ILessonService _lessonService;

    public QuizzesController(AppDbContext db, IQuizService quizService, ILessonService lessonService) : base(db)
    {
        _quizService = quizService;
        _lessonService = lessonService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, int? lessonId, int page = 1)
    {
        if (page < 1) page = 1;

        var (items, totalCount) = await _quizService.GetQuizzesAsync(keyword, lessonId, page, DefaultPageSize);

        ViewBag.Keyword = keyword;
        ViewBag.LessonId = lessonId;
        ViewBag.PageNumber = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)DefaultPageSize);

        var lessons = await _lessonService.GetAllLessonsAsync();
        var lessonList = lessons.Select(l => new {
            l.LessonId,
            DisplayText = $"{l.Course.CourseName} - {l.Title}"
        });
        ViewBag.LessonSelectList = new SelectList(lessonList, "LessonId", "DisplayText", lessonId);

        return View("~/Views/ContentManager/Quizzes/Index.cshtml", items);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(int? lessonId)
    {
        await PopulateLessonsDropdownAsync(lessonId);
        return View("~/Views/ContentManager/Quizzes/Create.cshtml", new QuizCreateViewModel { LessonId = lessonId ?? 0 });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(QuizCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Quizzes/Create.cshtml", model);
        }

        var (success, errorMessage) = await _quizService.CreateQuizAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo câu hỏi.");
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Quizzes/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "Thêm câu hỏi thành công.";
        return RedirectToAction(nameof(Index), new { lessonId = model.LessonId });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var (model, errorMessage) = await _quizService.GetQuizForEditAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        await PopulateLessonsDropdownAsync(model.LessonId);
        return View("~/Views/ContentManager/Quizzes/Edit.cshtml", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, QuizEditViewModel model)
    {
        if (id != model.QuizId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Quizzes/Edit.cshtml", model);
        }

        var (success, errorMessage) = await _quizService.UpdateQuizAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể cập nhật câu hỏi.");
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Quizzes/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "Cập nhật câu hỏi thành công.";
        return RedirectToAction(nameof(Index), new { lessonId = model.LessonId });
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _quizService.DeleteQuizAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"]
            = success ? "Đã xoá câu hỏi." : errorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLessonsDropdownAsync(int? selectedLessonId = null)
    {
        var lessons = await _lessonService.GetAllLessonsAsync();
        var lessonList = lessons.Select(l => new {
            l.LessonId,
            DisplayText = $"{l.Course.CourseName} - {l.Title}"
        });
        ViewBag.LessonSelectList = new SelectList(lessonList, "LessonId", "DisplayText", selectedLessonId);
    }
}
