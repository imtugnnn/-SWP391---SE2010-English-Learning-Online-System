using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IAssignmentRepository
{
    Task<List<Lesson>> GetPublishedLessonsByCourseIdAsync(int courseId);

    Task<List<Course>> GetPublishedCoursesAsync();

    Task<List<int>> GetAssignedLessonIdsAsync(
        int courseId,
        List<int> lessonIds,
        DateTime weekStartDate);

    Task AddWeeklyAssignmentsAsync(List<WeeklyAssignment> assignments);
    Task<bool> ValidateLessonsBelongToCourseAsync(int courseId, List<int> lessonIds);

    Task<List<WeeklyAssignment>> GetAssignmentsByCourseIdsAsync(List<int> courseIds);
}
