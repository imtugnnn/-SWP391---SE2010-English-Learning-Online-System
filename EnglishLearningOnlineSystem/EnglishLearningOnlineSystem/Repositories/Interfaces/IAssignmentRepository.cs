using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IAssignmentRepository
{
    Task<List<Lesson>> GetPublishedLessonsByCourseIdAsync(int courseId);

    Task<List<int>> GetAssignedLessonIdsAsync(
        int courseId,
        List<int> lessonIds,
        DateTime weekStartDate);

    Task AddWeeklyAssignmentsAsync(List<WeeklyAssignment> assignments);

    Task<List<WeeklyAssignment>> GetAssignmentsByCourseIdsAsync(List<int> courseIds);
}