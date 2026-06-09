using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IStudentDashboardService
{
    Task<StudentDashboardViewModel?> GetDashboardAsync(int userId);
    Task EnsureStudentProfileAsync(int userId);
}
