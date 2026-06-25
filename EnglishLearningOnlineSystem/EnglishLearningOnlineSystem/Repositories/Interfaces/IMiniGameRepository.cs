using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IMiniGameRepository
{
    Task<(IEnumerable<MiniGame> Items, int TotalCount)> GetPagedAsync(
        int? lessonId,
        string? searchTitle,
        int page,
        int pageSize);

    Task<MiniGame?> GetByIdAsync(int gameId);
    Task<MiniGame?> GetByIdWithLessonAsync(int gameId);
    Task<IEnumerable<MiniGame>> GetByLessonIdAsync(int lessonId);
    Task AddAsync(MiniGame game);
    void Update(MiniGame game);
    void Delete(MiniGame game);
    Task<bool> ExistsAsync(int gameId);
    Task SaveChangesAsync();
}