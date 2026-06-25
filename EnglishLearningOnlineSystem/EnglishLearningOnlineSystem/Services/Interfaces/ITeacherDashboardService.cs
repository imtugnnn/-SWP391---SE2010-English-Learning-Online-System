using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ITeacherDashboardService
{
    Task<TeacherDashboardViewModel> GetTeacherDashboardAsync(int teacherId);
}