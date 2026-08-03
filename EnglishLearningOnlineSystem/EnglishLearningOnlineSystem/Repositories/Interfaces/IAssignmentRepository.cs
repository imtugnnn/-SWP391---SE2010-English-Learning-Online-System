using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IAssignmentRepository
{
    Task<List<Lesson>> GetPublishedLessonsByCourseIdAsync(int courseId);
    Task<Lesson?> GetPublishedLessonDetailAsync(int courseId, int lessonId);

    Task<List<Course>> GetPublishedCoursesAsync();

    Task<List<int>> GetAssignedLessonIdsAsync(
        int classId,
        int courseId,
        List<int> lessonIds,
        DateTime weekStartDate);

    Task AddWeeklyAssignmentsAsync(List<WeeklyAssignment> assignments);
    Task<int> CountPublishedLessonsAsync(int courseId, List<int> lessonIds);

    Task<List<WeeklyAssignment>> GetAssignmentsByClassIdsAsync(List<int> classIds);
    Task<WeeklyAssignment?> GetForUpdateAsync(int assignmentId, int classId, int courseId);
    Task<bool> ExistsPublishedAssignmentAsync(
        int classId,
        int courseId,
        int? lessonId,
        DateTime weekStartDate,
        int excludedAssignmentId);
    Task<WeeklyAssignment?> GetAssignmentDetailsAsync(int assignmentId, int classId);
    Task<bool> HasStudentProgressAsync(int assignmentId);
    void RemoveAssignment(WeeklyAssignment assignment);
}
