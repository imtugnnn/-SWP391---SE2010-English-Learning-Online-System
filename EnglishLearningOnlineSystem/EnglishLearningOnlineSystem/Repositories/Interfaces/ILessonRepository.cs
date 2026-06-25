using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface ILessonRepository
{
    Task<List<Lesson>> GetAllLessonsWithCourseAsync();

    // Content Manager CRUD
    Task<(List<Lesson> Lessons, int TotalCount)> GetLessonsPaginatedAsync(string? keyword, int? courseId, int page, int pageSize);
    Task<Lesson?> GetLessonByIdAsync(int id);
    Task AddLessonAsync(Lesson lesson);
    Task UpdateLessonAsync(Lesson lesson);
    Task DeleteLessonAsync(Lesson lesson);
}
