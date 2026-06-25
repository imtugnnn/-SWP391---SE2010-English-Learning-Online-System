using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

[Route("contentmanager/vocabularies")]
public class VocabulariesController : BaseContentManagerController
{
    private const int DefaultPageSize = 10;
    private readonly IVocabularyService _vocabularyService;
    private readonly ILessonService _lessonService;

    public VocabulariesController(AppDbContext db, IVocabularyService vocabularyService, ILessonService lessonService) : base(db)
    {
        _vocabularyService = vocabularyService;
        _lessonService = lessonService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, int? lessonId, int page = 1)
    {
        if (page < 1) page = 1;

        var (items, totalCount) = await _vocabularyService.GetVocabulariesAsync(keyword, lessonId, page, DefaultPageSize);

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

        return View("~/Views/ContentManager/Vocabularies/Index.cshtml", items);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(int? lessonId)
    {
        await PopulateLessonsDropdownAsync(lessonId);
        return View("~/Views/ContentManager/Vocabularies/Create.cshtml", new VocabularyCreateViewModel { LessonId = lessonId ?? 0 });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VocabularyCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Vocabularies/Create.cshtml", model);
        }

        var (success, errorMessage) = await _vocabularyService.CreateVocabularyAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo từ vựng.");
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Vocabularies/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "Thêm từ vựng thành công.";
        return RedirectToAction(nameof(Index), new { lessonId = model.LessonId });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var (model, errorMessage) = await _vocabularyService.GetVocabularyForEditAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        await PopulateLessonsDropdownAsync(model.LessonId);
        return View("~/Views/ContentManager/Vocabularies/Edit.cshtml", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, VocabularyEditViewModel model)
    {
        if (id != model.VocabularyId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Vocabularies/Edit.cshtml", model);
        }

        var (success, errorMessage) = await _vocabularyService.UpdateVocabularyAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể cập nhật từ vựng.");
            await PopulateLessonsDropdownAsync(model.LessonId);
            return View("~/Views/ContentManager/Vocabularies/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "Cập nhật từ vựng thành công.";
        return RedirectToAction(nameof(Index), new { lessonId = model.LessonId });
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _vocabularyService.DeleteVocabularyAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"]
            = success ? "Đã xoá từ vựng." : errorMessage;

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
