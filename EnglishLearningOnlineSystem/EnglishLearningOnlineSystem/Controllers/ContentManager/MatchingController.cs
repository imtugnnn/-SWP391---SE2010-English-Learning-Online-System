using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Student.Games;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Student;

public class MatchingController : Controller
{
    private readonly IMatchingGameService _matchingGameService;

    public MatchingController(IMatchingGameService matchingGameService)
    {
        _matchingGameService = matchingGameService;
    }

    // GET: /Matching/Play/5
    // Tải màn chơi: chọn ngẫu nhiên các cặp từ vựng và xáo trộn cột nghĩa
    [HttpGet("/Matching/Play/{id:int}")]
    public async Task<IActionResult> Play(int id)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _matchingGameService.LoadPlayAsync(id);

        if (vm == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy trò chơi hoặc bài học này chưa đủ từ vựng (cần tối thiểu 2 từ).";
            return Redirect("/student/lessons");
        }

        return View("~/Views/Student/Games/MatchingPlay.cshtml", vm);
    }

    // POST: /Matching/Submit
    // Kiểm tra các cặp ghép và lưu kết quả
    [HttpPost("/Matching/Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(MatchingSubmitViewModel vm)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var studentId = GetCurrentStudentId();
        var (result, error) = await _matchingGameService.SubmitAsync(vm, studentId);

        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return Redirect($"/Matching/Play/{vm.GameId}");
        }

        return View("~/Views/Student/Games/MatchingResult.cshtml", result);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private bool IsAuthorized()
    {
        var role = HttpContext.Session.GetString("UserRole");
        return int.TryParse(role, out var roleId) && roleId == 1;
    }

    private IActionResult RedirectToLogin() =>
        RedirectToAction("Login", "Auth");

    private int GetCurrentStudentId()
    {
        var str = HttpContext.Session.GetString("UserId");
        return int.TryParse(str, out var id) ? id : 0;
    }
}
