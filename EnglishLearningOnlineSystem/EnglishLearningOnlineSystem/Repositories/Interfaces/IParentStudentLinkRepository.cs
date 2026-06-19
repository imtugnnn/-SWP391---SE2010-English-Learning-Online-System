using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IParentStudentLinkRepository
{
    Task<List<ParentStudentLink>> GetByParentIdAsync(int parentId);
    Task<ParentStudentLink?> GetByIdAsync(int id);
    Task<bool> LinkExistsAsync(int parentId, int studentId);
    Task AddAsync(ParentStudentLink link);
    Task DeleteAsync(ParentStudentLink link);

    Task<StudentProfile?> GetLinkedStudentProfileAsync(int parentId, int studentId);
    Task<int> CountCompletedLessonsAsync(int studentId);
    Task<int> CountBadgesAsync(int studentId);
    Task<double?> GetAverageQuizScoreAsync(int studentId);
    Task<List<Progress>> GetRecentProgressAsync(int studentId, int take);
    Task<List<WeeklyAssignment>> GetUpcomingAssignmentsAsync(int take);
    Task<List<StudentBadge>> GetRecentBadgesAsync(int studentId, int take);
}
