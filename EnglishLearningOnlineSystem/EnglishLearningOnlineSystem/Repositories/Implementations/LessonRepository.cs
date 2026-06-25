using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class LessonRepository : ILessonRepository
{
    private readonly AppDbContext _db;

    public LessonRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Lesson>> GetAllLessonsWithCourseAsync()
    {
        return await _db.Lessons!
            .Include(l => l.Course)
            .OrderBy(l => l.Course.CourseName)
            .ThenBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task<(List<Lesson> Lessons, int TotalCount)> GetLessonsPaginatedAsync(string? keyword, int? courseId, int page, int pageSize)
    {
        var query = _db.Lessons!
            .Include(l => l.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(l => l.Title.Contains(keyword) || l.Topic.Contains(keyword));
        }

        if (courseId.HasValue)
        {
            query = query.Where(l => l.CourseId == courseId.Value);
        }

        int totalCount = await query.CountAsync();

        var lessons = await query
            .OrderByDescending(l => l.LessonId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (lessons, totalCount);
    }

    public async Task<Lesson?> GetLessonByIdAsync(int id)
    {
        return await _db.Lessons!.FirstOrDefaultAsync(l => l.LessonId == id);
    }

    public async Task AddLessonAsync(Lesson lesson)
    {
        _db.Lessons!.Add(lesson);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLessonAsync(Lesson lesson)
    {
        _db.Lessons!.Update(lesson);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteLessonAsync(Lesson lesson)
    {
        _db.Lessons!.Remove(lesson);
        await _db.SaveChangesAsync();
    }
}
