using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ITeacherDashboardService
{
    /// <summary>
    /// Tổng hợp dữ liệu trang dashboard của giáo viên.
    /// </summary>
    Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(int teacherId);
}
