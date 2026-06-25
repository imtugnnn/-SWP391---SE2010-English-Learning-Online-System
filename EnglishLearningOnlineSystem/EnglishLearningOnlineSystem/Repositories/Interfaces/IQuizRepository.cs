using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IQuizRepository
{
    Task<(List<Quiz> Items, int TotalCount)> GetQuizzesPaginatedAsync(string? keyword, int? lessonId, int page, int pageSize);
    Task<Quiz?> GetQuizByIdAsync(int id);
    Task AddQuizAsync(Quiz quiz);
    Task UpdateQuizAsync(Quiz quiz);
    Task DeleteQuizAsync(Quiz quiz);
}
