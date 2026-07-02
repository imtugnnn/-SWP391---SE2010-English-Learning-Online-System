using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ITeacherAssignmentService
{
    Task<AssignWeeklyLessonViewModel?> GetAssignWeeklyLessonsFormAsync(
    int classId,
    int teacherId,
    int? selectedCourseId = null);

    Task<bool> AssignWeeklyLessonsAsync(
        AssignWeeklyLessonViewModel model,
        int teacherId);
    Task<TeacherAssignmentOverviewViewModel> GetAssignmentOverviewAsync(
    int? classId,
    int teacherId,
    string? status,
    string? sortBy,
    int page);
}