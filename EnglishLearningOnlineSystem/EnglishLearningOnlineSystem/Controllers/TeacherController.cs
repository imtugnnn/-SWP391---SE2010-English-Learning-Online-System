using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EnglishLearningOnlineSystem.Models;
using System;
using System.Threading.Tasks;
using System.Linq;

namespace EnglishLearningOnlineSystem.Controllers;

public class TeacherController : Controller
{
    private readonly IClassService _classService;
    private readonly IStudentManagementService _studentManagementService;
    private readonly ITeacherAssignmentService _teacherAssignmentService;
    private readonly ITeacherDashboardService _teacherDashboardService;
    private readonly EnglishLearningOnlineSystem.Data.AppDbContext _context;

    public TeacherController(
        IClassService classService,
        IStudentManagementService studentManagementService,
        ITeacherAssignmentService teacherAssignmentService,
        ITeacherDashboardService teacherDashboardService,
        EnglishLearningOnlineSystem.Data.AppDbContext context)
    {
        _classService = classService;
        _studentManagementService = studentManagementService;
        _teacherAssignmentService = teacherAssignmentService;
        _teacherDashboardService = teacherDashboardService;
        _context = context;
    }
    public async Task<IActionResult> Dashboard()
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _teacherDashboardService.GetTeacherDashboardAsync(teacherId.Value);

        return View(viewModel);
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

        await LogActivityAsync(teacherId.Value, $"Gửi phản hồi cho học sinh (ID: {model.StudentId}) tại lớp (ID: {model.ClassId})");

        TempData["SuccessMessage"] = "Phản hồi đã được gửi thành công.";

        return RedirectToAction(
            nameof(StudentDetail),
            new
            {
                classId = model.ClassId,
                studentId = model.StudentId
            });
    }
    [HttpGet]
    public async Task<IActionResult> AssignWeeklyLessons(int classId, int? selectedCourseId)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _teacherAssignmentService.GetAssignWeeklyLessonsFormAsync(
            classId,
            teacherId.Value,
            selectedCourseId);

        if (viewModel == null)
        {
            return Content("Không tìm thấy lớp học hoặc bạn không có quyền giao bài cho lớp này.");
        }

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignWeeklyLessons(AssignWeeklyLessonViewModel model)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (model.SelectedLessonIds == null || !model.SelectedLessonIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một bài học.");
        }

        if (model.DueDate < model.WeekStartDate)
        {
            ModelState.AddModelError(string.Empty, "Hạn hoàn thành không được nhỏ hơn ngày bắt đầu.");
        }

        if (!ModelState.IsValid)
        {
            var reloadModel = await _teacherAssignmentService.GetAssignWeeklyLessonsFormAsync(
                model.ClassId,
                teacherId.Value);

            if (reloadModel == null)
            {
                return Content("Không thể tải lại dữ liệu giao bài.");
            }

            reloadModel.WeekStartDate = model.WeekStartDate;
            reloadModel.DueDate = model.DueDate;
            reloadModel.SelectedLessonIds = model.SelectedLessonIds ?? new List<int>();
            reloadModel.SelectedCourseId = model.SelectedCourseId;

            return View(reloadModel);
        }

        var success = await _teacherAssignmentService.AssignWeeklyLessonsAsync(
            model,
            teacherId.Value);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Không thể giao bài. Có thể các bài học đã được giao trong tuần này.");

            var reloadModel = await _teacherAssignmentService.GetAssignWeeklyLessonsFormAsync(
    model.ClassId,
    teacherId.Value,
    model.SelectedCourseId);

            if (reloadModel == null)
            {
                return Content("Không thể tải lại dữ liệu giao bài.");
            }

            reloadModel.WeekStartDate = model.WeekStartDate;
            reloadModel.DueDate = model.DueDate;
            reloadModel.SelectedLessonIds = model.SelectedLessonIds ?? new List<int>();

            return View(reloadModel);
        }

        if (!model.SelectedCourseId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn chương trình học trước khi giao bài.");
        }

        await LogActivityAsync(teacherId.Value, $"Giao bài học theo tuần cho lớp (ID: {model.ClassId}), các bài học ID: [{string.Join(", ", model.SelectedLessonIds ?? new List<int>())}] từ ngày {model.WeekStartDate:dd/MM/yyyy} đến ngày {model.DueDate:dd/MM/yyyy}");

        TempData["SuccessMessage"] = "Giao bài học theo tuần thành công.";

        return RedirectToAction(
            nameof(ClassDetail),
            new { classId = model.ClassId });
    }
    public async Task<IActionResult> AssignmentOverview(
    int? classId,
    string? status,
    string? sortBy,
    int page = 1)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _teacherAssignmentService.GetAssignmentOverviewAsync(
    classId,
    teacherId.Value,
    status,
    sortBy,
    page);


        return View(viewModel);
    }

    private async Task LogActivityAsync(int userId, string action)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == userId);
        if (user != null)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                Username = user.Username,
                UserRole = user.Role?.Name ?? "Teacher",
                Action = action,
                Timestamp = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
    }
}