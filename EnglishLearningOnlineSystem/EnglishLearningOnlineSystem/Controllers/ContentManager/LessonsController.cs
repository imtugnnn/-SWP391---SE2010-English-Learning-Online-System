using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

public class LessonsController : BaseContentManagerController
{
    private readonly ILessonService _lessonService;
    private const int PageSize = 10;

    public LessonsController(AppDbContext db, ILessonService lessonService)
        : base(db)
    {
        _lessonService = lessonService;
    }

    // GET: /ContentManager/Lessons
    public async Task<IActionResult> Index(int? courseId, string? searchTitle, int page = 1)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (page < 1) page = 1;

        var vm = await _lessonService.GetPagedAsync(courseId, searchTitle, page, PageSize);

        return View("~/Views/ContentManager/Lessons/Index.cshtml", vm);
    }

    // GET: /ContentManager/Lessons/Details/5
    public async Task<IActionResult> Details(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _lessonService.GetDetailsAsync(id);

        if (vm == null)
            return NotFound();

        return View("~/Views/ContentManager/Lessons/Details.cshtml", vm);
    }

    // GET: /ContentManager/Lessons/Create
    public async Task<IActionResult> Create(int courseId)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _lessonService.BuildCreateViewModelAsync(courseId);

        if (vm == null)
            return NotFound();

        return View("~/Views/ContentManager/Lessons/Create.cshtml", vm);
    }

    // POST: /ContentManager/Lessons/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateLessonViewModel vm)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (!ModelState.IsValid)
        {
            var vmRebuild = await _lessonService.BuildCreateViewModelAsync(vm.CourseId);
            vm.CourseName = vmRebuild?.CourseName ?? string.Empty;
            return View("~/Views/ContentManager/Lessons/Create.cshtml", vm);
        }

        var managerId = GetCurrentUserId();
        var (lessonId, error) = await _lessonService.CreateAsync(vm, managerId);

        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            var vmRebuild = await _lessonService.BuildCreateViewModelAsync(vm.CourseId);
            vm.CourseName = vmRebuild?.CourseName ?? string.Empty;
            return View("~/Views/ContentManager/Lessons/Create.cshtml", vm);
        }

        TempData["SuccessMessage"] = "Bài học đã được tạo thành công. Bạn có thể thêm Mini Game bên dưới.";
        return RedirectToAction(nameof(Details), new { id = lessonId });
    }

    // GET: /ContentManager/Lessons/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _lessonService.BuildEditViewModelAsync(id);
        if (vm == null) return NotFound();

        return View("~/Views/ContentManager/Lessons/Edit.cshtml", vm);
    }

    // POST: /ContentManager/Lessons/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditLessonViewModel vm)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (id != vm.LessonId) return BadRequest();

        if (!ModelState.IsValid)
        {
            var rebuilt = await _lessonService.BuildEditViewModelAsync(id);
            if (rebuilt != null)
            {
                vm.CourseName = rebuilt.CourseName;
                vm.Courses = rebuilt.Courses;
            }
            return View("~/Views/ContentManager/Lessons/Edit.cshtml", vm);
        }

        var error = await _lessonService.UpdateAsync(vm);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            var rebuilt = await _lessonService.BuildEditViewModelAsync(id);
            if (rebuilt != null)
            {
                vm.CourseName = rebuilt.CourseName;
                vm.Courses = rebuilt.Courses;
            }
            return View("~/Views/ContentManager/Lessons/Edit.cshtml", vm);
        }

        TempData["SuccessMessage"] = "Cập nhật bài học thành công.";
        return RedirectToAction(nameof(Details), new { id = vm.LessonId });
    }

    // POST: /ContentManager/Lessons/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? courseId)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var error = await _lessonService.DeleteAsync(id);

        if (error != null)
            TempData["ErrorMessage"] = error;
        else
            TempData["SuccessMessage"] = "Xóa bài học thành công.";

        return RedirectToAction(nameof(Index), new { courseId });
    }

    // POST: /ContentManager/Lessons/TogglePublished/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TogglePublished(int id, int? courseId)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var error = await _lessonService.TogglePublishedAsync(id);

        if (error != null)
            TempData["ErrorMessage"] = error;

        return RedirectToAction(nameof(Index), new { courseId });
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private bool IsAuthorized()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return int.TryParse(role, out var roleId) && (roleId == 5 || roleId == 2);
    }

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Auth");

    private int GetCurrentUserId()
    {
        var str = HttpContext.Session.GetString("UserId");
        return int.TryParse(str, out var id) ? id : 0;
    }
}