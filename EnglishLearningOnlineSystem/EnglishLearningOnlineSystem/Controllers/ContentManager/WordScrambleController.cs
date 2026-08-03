using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Student.Games;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers.Student;

public class WordScrambleController : Controller
{
    private readonly IWordScrambleService _wordScrambleService;

    public WordScrambleController(IWordScrambleService wordScrambleService)
    {
        _wordScrambleService = wordScrambleService;
    }

    // GET: /WordScramble/Play/5
    // Tải màn chơi: chọn ngẫu nhiên từ vựng và xáo trộn chữ cái
    [HttpGet("/WordScramble/Play/{id:int}")]
    public async Task<IActionResult> Play(int id, int? assignmentId = null)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        var vm = await _wordScrambleService.LoadPlayAsync(id, GetCurrentStudentId(), assignmentId);

        if (vm == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy trò chơi hoặc bài học này chưa có từ vựng.";
            return Redirect("/student/lessons");
        }

        return View("~/Views/Student/Games/Play.cshtml", vm);
    }

    // POST: /WordScramble/Submit
    // Kiểm tra đáp án và lưu kết quả
    [HttpPost("/WordScramble/Submit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(WordScrambleSubmitViewModel vm)
    {
        if (!IsAuthorized()) return RedirectToLogin();

        if (!ModelState.IsValid)
        {
            var playVm = await _wordScrambleService.LoadPlayAsync(
                vm.GameId, GetCurrentStudentId(), vm.AssignmentId);
            if (playVm == null) return NotFound();
            return View("~/Views/Student/Games/Play.cshtml", playVm);
        }

        var studentId = GetCurrentStudentId();
        var (result, error) = await _wordScrambleService.SubmitAsync(vm, studentId);

        if (error != null)
        {
            TempData["ErrorMessage"] = error;
            return Redirect($"/WordScramble/Play/{vm.GameId}?assignmentId={vm.AssignmentId}");
        }

        return View("~/Views/Student/Games/Result.cshtml", result);
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
