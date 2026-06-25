using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface ILessonService
{
    Task<List<Lesson>> GetAllLessonsAsync();

    Task<(List<EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonListItemViewModel> Items, int TotalCount)> GetLessonsAsync(string? keyword, int? courseId, int page, int pageSize);
    Task<(EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonEditViewModel? Model, string? ErrorMessage)> GetLessonForEditAsync(int id);
    Task<(bool Success, string? ErrorMessage)> CreateLessonAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonCreateViewModel model);
    Task<(bool Success, string? ErrorMessage)> UpdateLessonAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons.LessonEditViewModel model);
    Task<(bool Success, string? ErrorMessage)> DeleteLessonAsync(int id);
}
