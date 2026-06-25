using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.ContentManager;

public class MiniGamesController : BaseContentManagerController
{
    private readonly IMiniGameService _miniGameService;
    private const int PageSize = 10;

    public MiniGamesController(AppDbContext db, IMiniGameService miniGameService)
        : base(db)
    {
        _miniGameService = miniGameService;
    }

    // GET: /ContentManager/MiniGames
    public async Task<IActionResult> Index(int? lessonId, string? searchTitle, int page = 1)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (page < 1) page = 1;

        var vm = await _miniGameService.GetPagedAsync(lessonId, searchTitle, page, PageSize);

        return View("~/Views/ContentManager/MiniGames/Index.cshtml", vm);
    }

    // GET: /ContentManager/MiniGames/Details/5
    public async Task<IActionResult> Details(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _miniGameService.GetDetailsAsync(id);

        if (vm == null)
            return NotFound();

        return View("~/Views/ContentManager/MiniGames/Details.cshtml", vm);
    }

    // GET: /ContentManager/MiniGames/Create?lessonId=3
    public async Task<IActionResult> Create(int lessonId)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _miniGameService.BuildCreateViewModelAsync(lessonId);

        if (vm == null)
            return NotFound();

        return View("~/Views/ContentManager/MiniGames/Create.cshtml", vm);
    }

    // POST: /ContentManager/MiniGames/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMiniGameViewModel vm)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (!ModelState.IsValid)
        {
            var vmRebuild = await _miniGameService.BuildCreateViewModelAsync(vm.LessonId);
            if (vmRebuild != null) vm.LessonTitle = vmRebuild.LessonTitle;
            return View("~/Views/ContentManager/MiniGames/Create.cshtml", vm);
        }

        var error = await _miniGameService.CreateAsync(vm);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            var vmRebuild = await _miniGameService.BuildCreateViewModelAsync(vm.LessonId);
            if (vmRebuild != null) vm.LessonTitle = vmRebuild.LessonTitle;
            return View("~/Views/ContentManager/MiniGames/Create.cshtml", vm);
        }

        TempData["SuccessMessage"] = "Trò chơi đã được tạo thành công.";
        return RedirectToAction("Details", "Lessons", new { id = vm.LessonId });
    }

    // GET: /ContentManager/MiniGames/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _miniGameService.BuildEditViewModelAsync(id);
        if (vm == null) return NotFound();

        return View("~/Views/ContentManager/MiniGames/Edit.cshtml", vm);
    }

    // POST: /ContentManager/MiniGames/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditMiniGameViewModel vm)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (id != vm.GameId) return BadRequest();

        if (!ModelState.IsValid)
        {
            var rebuilt = await _miniGameService.BuildEditViewModelAsync(id);
            if (rebuilt != null)
            {
                vm.LessonTitle = rebuilt.LessonTitle;
                vm.LessonId = rebuilt.LessonId;
                vm.GameType = rebuilt.GameType;
            }
            return View("~/Views/ContentManager/MiniGames/Edit.cshtml", vm);
        }

        var error = await _miniGameService.UpdateAsync(vm);
        if (error != null)
        {
            ModelState.AddModelError(string.Empty, error);
            var rebuilt = await _miniGameService.BuildEditViewModelAsync(id);
            if (rebuilt != null)
            {
                vm.LessonTitle = rebuilt.LessonTitle;
                vm.LessonId = rebuilt.LessonId;
                vm.GameType = rebuilt.GameType;
            }
            return View("~/Views/ContentManager/MiniGames/Edit.cshtml", vm);
        }

        TempData["SuccessMessage"] = "Trò chơi đã được cập nhật thành công.";
        return RedirectToAction("Details", "Lessons", new { id = vm.LessonId });
    }

    // POST: /ContentManager/MiniGames/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int? lessonId)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        // Lấy lessonId từ game trước khi xóa để đảm bảo redirect đúng
        var resolvedLessonId = lessonId ?? await _miniGameService.GetLessonIdAsync(id);

        var error = await _miniGameService.DeleteAsync(id);

        if (error != null)
        {
            TempData["ErrorMessage"] = error;
        }
        else
        {
            TempData["SuccessMessage"] = "Trò chơi đã được xóa thành công.";
        }

        if (resolvedLessonId.HasValue)
            return RedirectToAction("Details", "Lessons", new { id = resolvedLessonId.Value });

        return RedirectToAction("Index", "Lessons");
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private bool IsAuthorized()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return int.TryParse(role, out var roleId) && (roleId == 5 || roleId == 2);
    }

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Auth");
}