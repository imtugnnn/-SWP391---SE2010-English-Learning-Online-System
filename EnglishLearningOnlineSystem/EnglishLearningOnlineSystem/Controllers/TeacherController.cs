using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Mvc;
using EnglishLearningOnlineSystem.Models;
using System;
using System.Threading.Tasks;

namespace EnglishLearningOnlineSystem.Controllers;

public class TeacherController : Controller
{
    private readonly IClassService _classService;
    private readonly IStudentManagementService _studentManagementService;
    private readonly ITeacherAssignmentService _teacherAssignmentService;
    private readonly ITeacherDashboardService _teacherDashboardService;

    public TeacherController(
        IClassService classService,
        IStudentManagementService studentManagementService,
        ITeacherAssignmentService teacherAssignmentService,
        ITeacherDashboardService teacherDashboardService)
    {
        _classService = classService;
        _studentManagementService = studentManagementService;
        _teacherAssignmentService = teacherAssignmentService;
        _teacherDashboardService = teacherDashboardService;
    }

    /// <summary>
    /// Hiển thị trang tổng quan của giáo viên cùng số liệu lớp học, bài giao và thông báo.
    /// </summary>
    public async Task<IActionResult> Dashboard()
    {
        // Chỉ cho phép tài khoản giáo viên đã đăng nhập truy cập chức năng.
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _teacherDashboardService.GetTeacherDashboardAsync(teacherId.Value);

        return View(viewModel);
    }

    /// <summary>
    /// Hiển thị chi tiết một lớp do giáo viên hiện tại phụ trách.
    /// </summary>
    /// <param name="classId">Mã lớp cần xem.</param>
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
            return NotFound();
        }

        return View(viewModel);
    }

    /// <summary>
    /// Lấy mã người dùng từ session và đồng thời xác nhận người dùng có role Teacher (RoleId = 3).
    /// </summary>
    /// <returns>Mã giáo viên hợp lệ; null nếu chưa đăng nhập hoặc không đúng role.</returns>
    private int? GetCurrentUserId()
    {
        var role = HttpContext.Session.GetString("UserRole");
        if (role != "3")
        {
            return null;
        }

        var raw = HttpContext.Session.GetString("UserId");
        return int.TryParse(raw, out var id) ? id : null;
    }

    /// <summary>
    /// Hiển thị danh sách học sinh trong lớp, có hỗ trợ tìm kiếm, lọc, sắp xếp và phân trang.
    /// </summary>
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
            return NotFound();
        }

        return View(viewModel);
    }

    /// <summary>
    /// Tổng hợp các học sinh cần giáo viên hỗ trợ theo lớp, nguyên nhân và cách sắp xếp.
    /// </summary>
    public async Task<IActionResult> StudentsNeedSupport(
        int classId,
        string? reason,
        string? sortBy)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var classDetail = await _classService.GetTeacherClassDetailAsync(classId, teacherId.Value);
        if (classDetail == null)
        {
            return NotFound();
        }

        var viewModel = await _studentManagementService.GetStudentsNeedSupportAsync(
            teacherId.Value,
            classId.ToString(),
            reason,
            sortBy);

        return View(viewModel);
    }

    /// <summary>
    /// Hiển thị hồ sơ và tiến độ học tập của một học sinh thuộc lớp giáo viên phụ trách.
    /// </summary>
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
            return NotFound();
        }

        return View(viewModel);
    }

    /// <summary>
    /// Chuẩn bị biểu mẫu để giáo viên gửi phản hồi cho học sinh.
    /// </summary>
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
            return NotFound();
        }

        return View(viewModel);
    }

    /// <summary>
    /// Kiểm tra và lưu phản hồi của giáo viên, sau đó quay lại trang chi tiết học sinh.
    /// </summary>
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

    /// <summary>
    /// Hiển thị biểu mẫu chọn các bài học sẽ giao cho lớp trong tuần.
    /// </summary>
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
            return NotFound();
        }

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> LessonPreview(
        int classId,
        int lessonId,
        int? selectedCourseId)
    {
        var teacherId = GetCurrentUserId();
        if (teacherId == null)
        {
            return Unauthorized();
        }

        var viewModel = await _teacherAssignmentService.GetLessonPreviewAsync(
            classId,
            lessonId,
            teacherId.Value,
            selectedCourseId);

        if (viewModel == null)
        {
            return NotFound();
        }

        return PartialView("_LessonPreviewContent", viewModel);
    }

    /// <summary>
    /// Kiểm tra dữ liệu và tạo các bài giao tuần ở trạng thái nháp hoặc đã phát hành.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignWeeklyLessons(AssignWeeklyLessonViewModel model)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!ModelState.IsValid)
        {
            // Luồng ModelState: Controller chỉ điều phối, Service chịu trách nhiệm dựng lại form.
            var reloadModel = await _teacherAssignmentService
                .RebuildAssignWeeklyLessonsFormAsync(model, teacherId.Value);

            if (reloadModel == null)
            {
                return NotFound();
            }

            return View(reloadModel);
        }

        // Luồng nghiệp vụ: Service validate, lưu dữ liệu và trả kết quả cho Controller.
        var result = await _teacherAssignmentService.AssignWeeklyLessonsAsync(
            model,
            teacherId.Value);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Không thể giao bài.");

            if (result.FormModel == null)
            {
                return NotFound();
            }
            return View(result.FormModel);
        }

        TempData["SuccessMessage"] = model.Status == AssignmentStatus.Draft
            ? "Đã lưu bài giao ở trạng thái bản nháp."
            : "Giao bài học theo tuần thành công.";

        return model.Status == AssignmentStatus.Draft
            ? RedirectToAction(nameof(AssignmentOverview), new { classId = model.ClassId, status = "draft" })
            : RedirectToAction(nameof(AssignmentOverview), new { classId = model.ClassId });
    }

    /// <summary>
    /// Hiển thị toàn bộ bài giao của giáo viên, có hỗ trợ lọc theo lớp/trạng thái và phân trang.
    /// </summary>
    public async Task<IActionResult> AssignmentOverview(
    int classId,
    string? status,
    string? sortBy,
    int page = 1)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var classDetail = await _classService.GetTeacherClassDetailAsync(classId, teacherId.Value);
        if (classDetail == null)
        {
            return NotFound();
        }

        var viewModel = await _teacherAssignmentService.GetAssignmentOverviewAsync(
    classId,
    teacherId.Value,
    status,
    sortBy,
    page);


        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PublishDraft(int assignmentId, int classId)
    {
        var teacherId = GetCurrentUserId();
        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var success = await _teacherAssignmentService.PublishDraftAsync(
            assignmentId,
            classId,
            teacherId.Value);

        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Bản nháp đã được xuất bản thành công."
            : "Không thể xuất bản bản nháp. Vui lòng kiểm tra lại lớp và trạng thái bài giao.";

        return RedirectToAction(
            nameof(AssignmentOverview),
            new { classId, status = "draft" });
    }

}
