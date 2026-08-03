using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IAssignmentProgressRepository
{
    Task<WeeklyAssignment?> GetAccessibleAssignmentAsync(int assignmentId, int studentId);
    Task<AssignmentProgress?> GetProgressAsync(int assignmentId, int studentId);
    Task<List<AssignmentActivityProgress>> GetActivityProgressesAsync(int assignmentId, int studentId);
    Task AddProgressAsync(AssignmentProgress progress);
    Task AddActivityProgressAsync(AssignmentActivityProgress progress);
    Task SaveChangesAsync();
}
