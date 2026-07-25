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

    /// <summary>
    /// Tạo các bài giao tuần cho lớp thuộc quyền quản lý của giáo viên.
    /// </summary>
    Task<bool> AssignWeeklyLessonsAsync(
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
}
