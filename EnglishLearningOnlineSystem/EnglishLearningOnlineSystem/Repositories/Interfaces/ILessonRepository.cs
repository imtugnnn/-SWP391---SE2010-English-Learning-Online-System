using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface ILessonRepository
{
    Task<(IEnumerable<Lesson> Items, int TotalCount)> GetPagedAsync(
        int? courseId,
        string? searchTitle,
        int page,
        int pageSize);

    Task<Lesson?> GetByIdAsync(int lessonId);
    Task<Lesson?> GetByIdWithCourseAsync(int lessonId);
    Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId);
    Task AddAsync(Lesson lesson);
    void Update(Lesson lesson);
    void Delete(Lesson lesson);
    Task<bool> ExistsAsync(int lessonId);
    Task SaveChangesAsync();
}