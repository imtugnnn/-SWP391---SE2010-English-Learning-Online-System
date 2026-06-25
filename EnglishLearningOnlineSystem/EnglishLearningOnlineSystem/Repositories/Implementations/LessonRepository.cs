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

    public async Task<(IEnumerable<Lesson> Items, int TotalCount)> GetPagedAsync(
        int? courseId,
        string? searchTitle,
        int page,
        int pageSize)
    {
        var query = _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .Where(l => l.Course != null && !l.Course.IsDeleted)
            .AsQueryable();

        if (courseId.HasValue)
            query = query.Where(l => l.CourseId == courseId.Value);

        if (!string.IsNullOrWhiteSpace(searchTitle))
            query = query.Where(l => l.Title.Contains(searchTitle));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(l => l.CourseId)
            .ThenBy(l => l.OrderIndex)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Lesson?> GetByIdAsync(int lessonId)
    {
        return await _db.Lessons!
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);
    }

    public async Task<Lesson?> GetByIdWithCourseAsync(int lessonId)
    {
        return await _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);
    }

    public async Task<IEnumerable<Lesson>> GetByCourseIdAsync(int courseId)
    {
        return await _db.Lessons!
            .AsNoTracking()
            .Where(l => l.CourseId == courseId)
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task AddAsync(Lesson lesson)
    {
        await _db.Lessons!.AddAsync(lesson);
    }

    public void Update(Lesson lesson)
    {
        _db.Lessons!.Update(lesson);
    }

    public void Delete(Lesson lesson)
    {
        _db.Lessons!.Remove(lesson);
    }

    public async Task<bool> ExistsAsync(int lessonId)
    {
        return await _db.Lessons!.AnyAsync(l => l.LessonId == lessonId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}