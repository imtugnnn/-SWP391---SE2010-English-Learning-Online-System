using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
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
    public async Task<IActionResult> StudentDetail(int classId, int studentId)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _studentManagementService.GetStudentDetailAsync(
            classId,
            studentId,
            teacherId.Value);

        if (viewModel == null)
        {
            return Content("Không tìm thấy học sinh hoặc bạn không có quyền xem thông tin học sinh này.");
        }

        return View(viewModel);
    }
    [HttpGet]
    public async Task<IActionResult> ProvideFeedback(int classId, int studentId)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _studentManagementService.GetProvideFeedbackFormAsync(
            classId,
            studentId,
            teacherId.Value);

        if (viewModel == null)
        {
            return Content("Không tìm thấy học sinh hoặc bạn không có quyền gửi phản hồi cho học sinh này.");
        }

        return View(viewModel);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProvideFeedback(ProvideStudentFeedbackViewModel model)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var success = await _studentManagementService.CreateStudentFeedbackAsync(
            model,
            teacherId.Value);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Không thể gửi phản hồi. Vui lòng kiểm tra lại thông tin.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công.";

        return RedirectToAction(
            nameof(StudentDetail),
            new
            {
                classId = model.ClassId,
                studentId = model.StudentId
            });
    }
}