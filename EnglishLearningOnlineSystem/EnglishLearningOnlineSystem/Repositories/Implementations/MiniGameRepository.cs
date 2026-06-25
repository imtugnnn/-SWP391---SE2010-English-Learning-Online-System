using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class MiniGameRepository : IMiniGameRepository
{
    private readonly AppDbContext _db;

    public MiniGameRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(IEnumerable<MiniGame> Items, int TotalCount)> GetPagedAsync(
        int? lessonId,
        string? searchTitle,
        int page,
        int pageSize)
    {
        var query = _db.MiniGames!
            .AsNoTracking()
            .Include(g => g.Lesson)
            .AsQueryable();

        if (lessonId.HasValue)
            query = query.Where(g => g.LessonId == lessonId.Value);

        if (!string.IsNullOrWhiteSpace(searchTitle))
            query = query.Where(g => g.Title.Contains(searchTitle));

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(g => g.LessonId)
            .ThenBy(g => g.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<MiniGame?> GetByIdAsync(int gameId)
    {
        return await _db.MiniGames!
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task<MiniGame?> GetByIdWithLessonAsync(int gameId)
    {
        return await _db.MiniGames!
            .AsNoTracking()
            .Include(g => g.Lesson)
            .FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    public async Task<IEnumerable<MiniGame>> GetByLessonIdAsync(int lessonId)
    {
        return await _db.MiniGames!
            .AsNoTracking()
            .Where(g => g.LessonId == lessonId)
            .OrderBy(g => g.Title)
            .ToListAsync();
    }

    public async Task AddAsync(MiniGame game)
    {
        await _db.MiniGames!.AddAsync(game);
    }

    public void Update(MiniGame game)
    {
        _db.MiniGames!.Update(game);
    }

    public void Delete(MiniGame game)
    {
        _db.MiniGames!.Remove(game);
    }

    public async Task<bool> ExistsAsync(int gameId)
    {
        return await _db.MiniGames!.AnyAsync(g => g.GameId == gameId);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
