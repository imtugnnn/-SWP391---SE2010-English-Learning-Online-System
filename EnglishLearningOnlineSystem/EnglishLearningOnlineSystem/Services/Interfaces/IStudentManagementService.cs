using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IStudentManagementService
{
    Task<ManageStudentListViewModel?> GetManageStudentListAsync(
        int classId,
        int teacherId,
        string? keyword,
        string? status,
        string? sortBy,
        int page);

    Task<TeacherStudentDetailViewModel?> GetStudentDetailAsync(
    int classId,
    int studentId,
    int teacherId);

    Task<ProvideStudentFeedbackViewModel?> GetProvideFeedbackFormAsync(
    int classId,
    int studentId,
    int teacherId);

    Task<bool> CreateStudentFeedbackAsync(
        ProvideStudentFeedbackViewModel model,
        int teacherId);
}