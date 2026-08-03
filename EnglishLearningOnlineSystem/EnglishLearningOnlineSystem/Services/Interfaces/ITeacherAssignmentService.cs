using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ITeacherAssignmentService
{
    /// <summary>
    /// Chuẩn bị dữ liệu cho biểu mẫu giao bài theo tuần.
    /// </summary>
    Task<AssignWeeklyLessonViewModel?> GetAssignWeeklyLessonsFormAsync(
    int classId,
    int teacherId,
    int? selectedCourseId = null);

    Task<AssignWeeklyLessonViewModel?> RebuildAssignWeeklyLessonsFormAsync(
        AssignWeeklyLessonViewModel postedModel,
        int teacherId);

    Task<TeacherLessonPreviewViewModel?> GetLessonPreviewAsync(
        int classId,
        int lessonId,
        int teacherId,
        int? selectedCourseId = null);

    /// <summary>
    /// Tạo các bài giao tuần cho lớp thuộc quyền quản lý của giáo viên.
    /// </summary>
    Task<TeacherAssignmentResult> AssignWeeklyLessonsAsync(
        AssignWeeklyLessonViewModel model,
        int teacherId);
    /// <summary>
    /// Lấy danh sách tổng quan bài giao của giáo viên.
    /// </summary>
    Task<TeacherAssignmentOverviewViewModel> GetAssignmentOverviewAsync(
    int? classId,
    int teacherId,
    string? status,
    string? sortBy,
    int page);

    /// <summary>
    /// Phát hành một bài giao đang ở trạng thái bản nháp.
    /// </summary>
    Task<bool> PublishDraftAsync(int assignmentId, int classId, int teacherId);
    Task<TeacherAssignmentDetailsViewModel?> GetAssignmentDetailsAsync(
        int assignmentId,
        int classId,
        int teacherId);
    Task<EditTeacherAssignmentViewModel?> GetEditAssignmentAsync(
        int assignmentId,
        int classId,
        int teacherId);
    Task<TeacherAssignmentCommandResult> UpdateAssignmentAsync(
        EditTeacherAssignmentViewModel model,
        int teacherId);
    Task<TeacherAssignmentCommandResult> CancelAssignmentAsync(
        int assignmentId,
        int classId,
        int teacherId);
    Task<TeacherAssignmentCommandResult> ArchiveAssignmentAsync(
        int assignmentId,
        int classId,
        int teacherId);
    Task<TeacherAssignmentCommandResult> DeleteAssignmentAsync(
        int assignmentId,
        int classId,
        int teacherId);
}

public sealed class TeacherAssignmentCommandResult
{
    public bool Succeeded { get; init; }
    public string Message { get; init; } = string.Empty;

    public static TeacherAssignmentCommandResult Success(string message) =>
        new() { Succeeded = true, Message = message };
    public static TeacherAssignmentCommandResult Failure(string message) =>
        new() { Message = message };
}

/// <summary>
/// Kết quả xử lý luồng giao bài để Controller biết cần chuyển trang
/// hay hiển thị lại form cùng thông báo lỗi từ Service.
/// </summary>
public sealed class TeacherAssignmentResult
{
    public bool Succeeded { get; init; }
    public string? ErrorMessage { get; init; }
    public AssignWeeklyLessonViewModel? FormModel { get; init; }

    public static TeacherAssignmentResult Success() => new() { Succeeded = true };

    public static TeacherAssignmentResult Failure(
        string errorMessage,
        AssignWeeklyLessonViewModel? formModel = null)
    {
        return new TeacherAssignmentResult
        {
            ErrorMessage = errorMessage,
            FormModel = formModel
        };
    }
}
