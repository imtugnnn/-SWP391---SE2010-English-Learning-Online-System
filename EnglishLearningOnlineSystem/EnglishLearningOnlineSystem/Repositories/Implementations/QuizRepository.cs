using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _db;

    public QuizRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(List<Quiz> Items, int TotalCount)> GetQuizzesPaginatedAsync(string? keyword, int? lessonId, int page, int pageSize)
    {
        var query = _db.Quizzes!
            .Include(q => q.Lesson)
            .ThenInclude(l => l.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(q => q.Question.ToLower().Contains(lowerKeyword));
        }

        if (lessonId.HasValue && lessonId.Value > 0)
        {
            query = query.Where(q => q.LessonId == lessonId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(q => q.QuizId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Quiz?> GetQuizByIdAsync(int id)
    {
        return await _db.Quizzes!
            .Include(q => q.Lesson)
            .FirstOrDefaultAsync(q => q.QuizId == id);
    }

    public async Task AddQuizAsync(Quiz quiz)
    {
        _db.Quizzes!.Add(quiz);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateQuizAsync(Quiz quiz)
    {
        _db.Quizzes!.Update(quiz);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteQuizAsync(Quiz quiz)
    {
        _db.Quizzes!.Remove(quiz);
        await _db.SaveChangesAsync();
    }
}
