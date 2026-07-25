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

        ViewBag.SystemNotifications = viewModel.SystemNotifications;
        ViewBag.PersonalNotifications = viewModel.PersonalNotifications;
        ViewBag.NotificationCount = viewModel.NotificationCount;

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
        string? classFilter,
        string? reason,
        string? sortBy)
    {
        var teacherId = GetCurrentUserId();

        if (teacherId == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var viewModel = await _studentManagementService.GetStudentsNeedSupportAsync(
            teacherId.Value,
            classFilter,
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

        if (model.SelectedLessonIds == null || !model.SelectedLessonIds.Any())
        {
            ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một bài học.");
        }

        if (!model.SelectedCourseId.HasValue)
        {
            ModelState.AddModelError(nameof(model.SelectedCourseId), "Vui lòng chọn chương trình học trước khi giao bài.");
        }

        if (model.DueDate <= model.WeekStartDate)
        {
            ModelState.AddModelError(string.Empty, "Hạn hoàn thành phải lớn hơn ngày bắt đầu.");
        }

        if (model.Status != AssignmentStatus.Draft &&
            model.Status != AssignmentStatus.Published)
        {
            ModelState.AddModelError(nameof(model.Status), "Trạng thái bài giao không hợp lệ.");
        }

        if (!ModelState.IsValid)
        {
            // Nạp lại danh sách khóa học/bài học để biểu mẫu lỗi vẫn hiển thị đủ dữ liệu.
            var reloadModel = await _teacherAssignmentService.GetAssignWeeklyLessonsFormAsync(
                model.ClassId,
                teacherId.Value);

            if (reloadModel == null)
            {
                return NotFound();
            }

            reloadModel.WeekStartDate = model.WeekStartDate;
            reloadModel.DueDate = model.DueDate;
            reloadModel.SelectedLessonIds = model.SelectedLessonIds ?? new List<int>();
            reloadModel.SelectedCourseId = model.SelectedCourseId;
            reloadModel.Status = model.Status;

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
                return NotFound();
            }

            reloadModel.WeekStartDate = model.WeekStartDate;
            reloadModel.DueDate = model.DueDate;
            reloadModel.SelectedLessonIds = model.SelectedLessonIds ?? new List<int>();
            reloadModel.Status = model.Status;

            return View(reloadModel);
        }

        TempData["SuccessMessage"] = model.Status == AssignmentStatus.Draft
            ? "Đã lưu bài giao ở trạng thái bản nháp."
            : "Giao bài học theo tuần thành công.";

        return model.Status == AssignmentStatus.Draft
            ? RedirectToAction(nameof(AssignmentOverview), new { classId = model.ClassId, status = "draft" })
            : RedirectToAction(nameof(ClassDetail), new { classId = model.ClassId });
    }

    /// <summary>
    /// Hiển thị toàn bộ bài giao của giáo viên, có hỗ trợ lọc theo lớp/trạng thái và phân trang.
    /// </summary>
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
