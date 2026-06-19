using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers;

public class ParentController : Controller
{
    private const int ParentRoleId = 4;

    private readonly IParentStudentLinkService _linkService;

    public ParentController(IParentStudentLinkService linkService)
    {
        _linkService = linkService;
    }

    public async Task<IActionResult> Dashboard(int? studentId)
    {
        var parentId = GetCurrentParentId();
        if (parentId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var vm = await _linkService.BuildDashboardAsync(parentId.Value, studentId);
        return View(vm);
    }

    public async Task<IActionResult> Index()
    {
        var parentId = GetCurrentParentId();
        if (parentId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var vm = new ParentLinkPageViewModel();
        await PopulateLinkedStudents(vm, parentId.Value);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(ParentLinkPageViewModel vm)
    {
        var parentId = GetCurrentParentId();
        if (parentId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        await PopulateLinkedStudents(vm, parentId.Value);

        if (string.IsNullOrWhiteSpace(vm.StudentCode))
        {
            ModelState.AddModelError(nameof(vm.StudentCode), "Vui lòng nhập mã học sinh hoặc mã mời.");
            return View(nameof(Index), vm);
        }

        var result = await _linkService.VerifyCodeAsync(vm.StudentCode);
        if (!result.Succeeded || result.Data == null)
        {
            ModelState.AddModelError(nameof(vm.StudentCode), result.ErrorMessage ?? "Mã không hợp lệ.");
            return View(nameof(Index), vm);
        }

        vm.VerifiedStudent = result.Data;
        return View(nameof(Index), vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Link(ParentLinkPageViewModel vm)
    {
        var parentId = GetCurrentParentId();
        if (parentId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _linkService.LinkByCodeAsync(parentId.Value, vm.StudentCode, vm.Relationship);
        if (!result.Succeeded)
        {
            await PopulateLinkedStudents(vm, parentId.Value);
            ModelState.AddModelError("", result.ErrorMessage ?? "Liên kết thất bại.");

            var verify = await _linkService.VerifyCodeAsync(vm.StudentCode);
            vm.VerifiedStudent = verify.Data;
            return View(nameof(Index), vm);
        }

        TempData["Message"] = "Liên kết tài khoản với học sinh thành công.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlink(int linkId)
    {
        var parentId = GetCurrentParentId();
        if (parentId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _linkService.UnlinkStudentAsync(parentId.Value, linkId);
        TempData["Message"] = result.Succeeded
            ? "Đã hủy liên kết học sinh."
            : result.ErrorMessage ?? "Hủy liên kết thất bại.";

        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentParentId()
    {
        var userIdStr = HttpContext.Session.GetString("UserId");
        var roleStr = HttpContext.Session.GetString("UserRole");

        if (int.TryParse(userIdStr, out var userId)
            && int.TryParse(roleStr, out var roleId)
            && roleId == ParentRoleId)
        {
            return userId;
        }

        return null;
    }

    private async Task PopulateLinkedStudents(ParentLinkPageViewModel vm, int parentId)
    {
        var result = await _linkService.GetLinkedStudentsAsync(parentId);
        vm.LinkedStudents = result.Data ?? new List<LinkedStudentItem>();
    }
}
