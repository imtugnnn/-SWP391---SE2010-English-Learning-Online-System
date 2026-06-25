using EnglishLearningOnlineSystem.ViewModels;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ILessonService
{
    Task<LessonListViewModel> GetPagedAsync(
        int? courseId,
        string? searchTitle,
        int page,
        int pageSize);

    Task<LessonViewModel?> GetByIdAsync(int lessonId);
    Task<LessonDetailsViewModel?> GetDetailsAsync(int lessonId);
    Task<CreateLessonViewModel> BuildCreateViewModelAsync(int? preselectedCourseId = null);
    Task<EditLessonViewModel?> BuildEditViewModelAsync(int lessonId);

    /// <returns>null on success, error message string on failure.</returns>
    Task<(int LessonId, string? Error)> CreateAsync(CreateLessonViewModel vm, int creatorId);
    /// <returns>null on success, error message string on failure.</returns>
    Task<string?> UpdateAsync(EditLessonViewModel vm);

    /// <returns>null on success, error message string on failure.</returns>
    Task<string?> DeleteAsync(int lessonId);

    /// <returns>null on success, error message string on failure.</returns>
    Task<string?> TogglePublishedAsync(int lessonId);
}