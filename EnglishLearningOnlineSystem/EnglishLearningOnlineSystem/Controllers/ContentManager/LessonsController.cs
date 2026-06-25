using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

[Route("contentmanager/lessons")]
public class LessonsController : BaseContentManagerController
{
    private const int DefaultPageSize = 10;
    private readonly ILessonService _lessonService;
    private readonly ICourseService _courseService;

    public LessonsController(AppDbContext db, ILessonService lessonService, ICourseService courseService) : base(db)
    {
        _lessonService = lessonService;
        _courseService = courseService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? keyword, int? courseId, int page = 1)
    {
        if (page < 1) page = 1;

        var (items, totalCount) = await _lessonService.GetLessonsAsync(keyword, courseId, page, DefaultPageSize);

        ViewBag.Keyword = keyword;
        ViewBag.CourseId = courseId;
        ViewBag.PageNumber = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)DefaultPageSize);

        var courses = await _courseService.GetAllCoursesAsync();
        ViewBag.CourseSelectList = new SelectList(courses, "CourseId", "CourseName", courseId);

        return View("~/Views/ContentManager/Lessons/Index.cshtml", items);
    }

    [HttpGet("create")]
    public async Task<IActionResult> Create(int? courseId)
    {
        await PopulateCoursesDropdownAsync(courseId);
        return View("~/Views/ContentManager/Lessons/Create.cshtml", new LessonCreateViewModel { CourseId = courseId ?? 0 });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LessonCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCoursesDropdownAsync(model.CourseId);
            return View("~/Views/ContentManager/Lessons/Create.cshtml", model);
        }

        var (success, errorMessage) = await _lessonService.CreateLessonAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể tạo bài học.");
            await PopulateCoursesDropdownAsync(model.CourseId);
            return View("~/Views/ContentManager/Lessons/Create.cshtml", model);
        }

        TempData["SuccessMessage"] = "Thêm bài học thành công.";
        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }

    [HttpGet("{id:int}/edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var (model, errorMessage) = await _lessonService.GetLessonForEditAsync(id);

        if (model == null)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(Index));
        }

        await PopulateCoursesDropdownAsync(model.CourseId);
        return View("~/Views/ContentManager/Lessons/Edit.cshtml", model);
    }

    [HttpPost("{id:int}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, LessonEditViewModel model)
    {
        if (id != model.LessonId) return BadRequest();

        if (!ModelState.IsValid)
        {
            await PopulateCoursesDropdownAsync(model.CourseId);
            return View("~/Views/ContentManager/Lessons/Edit.cshtml", model);
        }

        var (success, errorMessage) = await _lessonService.UpdateLessonAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Không thể cập nhật bài học.");
            await PopulateCoursesDropdownAsync(model.CourseId);
            return View("~/Views/ContentManager/Lessons/Edit.cshtml", model);
        }

        TempData["SuccessMessage"] = "Cập nhật bài học thành công.";
        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }

    [HttpPost("{id:int}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var (success, errorMessage) = await _lessonService.DeleteLessonAsync(id);

        TempData[success ? "SuccessMessage" : "ErrorMessage"]
            = success ? "Đã xoá bài học." : errorMessage;

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateCoursesDropdownAsync(int? selectedCourseId = null)
    {
        var courses = await _courseService.GetAllCoursesAsync();
        ViewBag.CourseSelectList = new SelectList(courses, "CourseId", "CourseName", selectedCourseId);
    }
}
