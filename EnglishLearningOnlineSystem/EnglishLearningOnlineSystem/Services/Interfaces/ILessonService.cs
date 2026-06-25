using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ILessonService
{
    Task<List<Lesson>> GetAllLessonsAsync();

    Task<(List<LessonListItemViewModel> Items, int TotalCount)> GetLessonsAsync(
        string? keyword, int? courseId, int page, int pageSize);

    Task<LessonDetailsViewModel?> GetDetailsAsync(int lessonId);

    Task<(LessonEditViewModel? Model, string? ErrorMessage)> GetLessonForEditAsync(int id);

    Task<(bool Success, string? ErrorMessage)> CreateLessonAsync(LessonCreateViewModel model);

    Task<(bool Success, string? ErrorMessage)> UpdateLessonAsync(LessonEditViewModel model);

    Task<(bool Success, string? ErrorMessage)> DeleteLessonAsync(int id);

    Task<(bool Success, string? ErrorMessage)> TogglePublishedAsync(int lessonId);
}