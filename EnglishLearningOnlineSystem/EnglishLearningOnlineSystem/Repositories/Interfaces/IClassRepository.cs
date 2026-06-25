using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IClassRepository
{
    Task<Class?> GetClassDetailByIdAsync(int classId);
    Task<List<ClassEnrollment>> GetActiveStudentsByClassIdAsync(int classId);
    Task<List<WeeklyAssignment>> GetAssignmentsByClassCourseAsync(int? courseId);
    Task<List<Progress>> GetProgressByStudentIdsAndLessonIdsAsync(List<int> studentIds, List<int> lessonIds);
}