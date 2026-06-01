using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IStudentDashboardRepository
{
    Task<StudentProfile?> GetProfileByUserIdAsync(int userId);
    Task<List<WeeklyAssignment>> GetCurrentWeekAssignmentsAsync(int studentId);
    Task<List<Progress>> GetRecentProgressAsync(int studentId, int take = 5);
    Task<List<StudentMission>> GetTodayMissionsAsync(int studentId);
    Task<List<StudentBadge>> GetRecentBadgesAsync(int studentId, int take = 3);
    Task<int> GetTotalLessonsCompletedAsync(int studentId);
    Task UpdateLastActiveDateAsync(int studentId);
}
