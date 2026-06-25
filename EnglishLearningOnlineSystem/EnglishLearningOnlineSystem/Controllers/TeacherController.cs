using EnglishLearningOnlineSystem.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EnglishLearningOnlineSystem.Controllers;

public class TeacherController : Controller
{
    private readonly IClassService _classService;
    private readonly IStudentManagementService _studentManagementService;

    public TeacherController(IClassService classService, IStudentManagementService studentManagementService)
    {
        _classService = classService;
        _studentManagementService = studentManagementService;
    }

    public async Task<IActionResult> ClassDetail(int classId)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _classService.GetTeacherClassDetailAsync(classId, teacherId.Value);

        if (viewModel == null)
        {
            return Content("Không tìm thấy lớp học hoặc bạn không có quyền truy cập lớp này.");
        }

        return View(viewModel);
    }

    private int? GetCurrentUserId()
    {
        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }
    public async Task<IActionResult> ManageStudentList(
    int classId,
    string? keyword,
    string? status,
    string? sortBy,
    int page = 1)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _studentManagementService.GetManageStudentListAsync(
            classId,
            teacherId.Value,
            keyword,
            status,
            sortBy,
            page);

        if (viewModel == null)
        {
            return Content("Không tìm thấy lớp học hoặc bạn không có quyền quản lý danh sách học sinh của lớp này.");
        }

        return View(viewModel);
    }
}