using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IStudentManagementService
{
    /// <summary>
    /// Lấy danh sách học sinh trong một lớp do giáo viên phụ trách.
    /// </summary>
    Task<ManageStudentListViewModel?> GetManageStudentListAsync(
        int classId,
        int teacherId,
        string? keyword,
        string? status,
        string? sortBy,
        int page);

    /// <summary>
    /// Lấy danh sách học sinh có dấu hiệu cần hỗ trợ.
    /// </summary>
    Task<TeacherStudentsNeedSupportViewModel> GetStudentsNeedSupportAsync(
        int teacherId,
        string? classFilter,
        string? reasonFilter,
        string? sortBy);

    /// <summary>
    /// Đếm học sinh cần hỗ trợ trong tất cả lớp của giáo viên.
    /// </summary>
    Task<int> CountStudentsNeedSupportAsync(int teacherId);

    /// <summary>
    /// Lấy thông tin chi tiết và tiến độ của học sinh.
    /// </summary>
    Task<TeacherStudentDetailViewModel?> GetStudentDetailAsync(
    int classId,
    int studentId,
    int teacherId);

    /// <summary>
    /// Chuẩn bị dữ liệu biểu mẫu phản hồi cho học sinh.
    /// </summary>
    Task<ProvideStudentFeedbackViewModel?> GetProvideFeedbackFormAsync(
    int classId,
    int studentId,
    int teacherId);

    /// <summary>
    /// Lưu phản hồi của giáo viên cho học sinh.
    /// </summary>
    Task<bool> CreateStudentFeedbackAsync(
        ProvideStudentFeedbackViewModel model,
        int teacherId);
}
