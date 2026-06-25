using EnglishLearningOnlineSystem.ViewModels.ContentManager.Quizzes;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IQuizService
{
    Task<(List<QuizListItemViewModel> Items, int TotalCount)> GetQuizzesAsync(string? keyword, int? lessonId, int page, int pageSize);
    Task<(QuizEditViewModel? Model, string? ErrorMessage)> GetQuizForEditAsync(int id);
    Task<(bool Success, string? ErrorMessage)> CreateQuizAsync(QuizCreateViewModel model);
    Task<(bool Success, string? ErrorMessage)> UpdateQuizAsync(QuizEditViewModel model);
    Task<(bool Success, string? ErrorMessage)> DeleteQuizAsync(int id);
}
