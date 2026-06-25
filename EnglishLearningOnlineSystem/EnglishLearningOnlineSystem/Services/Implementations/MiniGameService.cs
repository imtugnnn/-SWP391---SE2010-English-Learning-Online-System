using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class MiniGameService : IMiniGameService
{
    private readonly IMiniGameRepository _gameRepo;
    private readonly AppDbContext _db;

    public MiniGameService(IMiniGameRepository gameRepo, AppDbContext db)
    {
        _gameRepo = gameRepo;
        _db = db;
    }

    public async Task<MiniGameListViewModel> GetPagedAsync(
        int? lessonId,
        string? searchTitle,
        int page,
        int pageSize)
    {
        var (items, total) = await _gameRepo.GetPagedAsync(lessonId, searchTitle, page, pageSize);

        var lessons = await GetActiveLessonSelectItemsAsync();

        return new MiniGameListViewModel
        {
            Items = items.Select(g => new MiniGameListItemViewModel
            {
                GameId = g.GameId,
                Title = g.Title,
                GameType = g.GameType,
                XPReward = g.XPReward,
                LessonId = g.LessonId,
                LessonTitle = g.Lesson?.Title ?? "—"
            }).ToList(),
            TotalCount = total,
            CurrentPage = page,
            PageSize = pageSize,
            SearchTitle = searchTitle,
            FilterLessonId = lessonId,
            Lessons = lessons
        };
    }

    public async Task<MiniGameViewModel?> GetByIdAsync(int gameId)
    {
        var game = await _gameRepo.GetByIdWithLessonAsync(gameId);
        if (game == null) return null;

        return MapToViewModel(game);
    }

    public async Task<MiniGameDetailsViewModel?> GetDetailsAsync(int gameId)
    {
        var game = await _db.MiniGames!
            .AsNoTracking()
            .Include(g => g.Lesson)
                .ThenInclude(l => l!.Course)
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null) return null;

        return new MiniGameDetailsViewModel
        {
            GameId = game.GameId,
            Title = game.Title,
            GameType = game.GameType,
            XPReward = game.XPReward,
            LessonId = game.LessonId,
            LessonTitle = game.Lesson?.Title ?? "—",
            CourseName = game.Lesson?.Course?.CourseName ?? "—"
        };
    }

    public async Task<CreateMiniGameViewModel?> BuildCreateViewModelAsync(int lessonId)
    {
        var lesson = await _db.Lessons!
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);

        if (lesson == null) return null;

        return new CreateMiniGameViewModel
        {
            LessonId = lesson.LessonId,
            LessonTitle = lesson.Title
        };
    }

    public async Task<EditMiniGameViewModel?> BuildEditViewModelAsync(int gameId)
    {
        var game = await _gameRepo.GetByIdWithLessonAsync(gameId);
        if (game == null) return null;

        return new EditMiniGameViewModel
        {
            GameId = game.GameId,
            LessonId = game.LessonId,
            LessonTitle = game.Lesson?.Title ?? "—",
            Title = game.Title,
            GameType = game.GameType,
            XPReward = game.XPReward
        };
    }

    public async Task<int?> GetLessonIdAsync(int gameId)
    {
        return await _db.MiniGames!
            .AsNoTracking()
            .Where(g => g.GameId == gameId)
            .Select(g => (int?)g.LessonId)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> CreateAsync(CreateMiniGameViewModel vm)
    {
        var lesson = await _db.Lessons!
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LessonId == vm.LessonId);

        if (lesson == null)
            return "Bài học được chọn không tồn tại.";

        var allowedGameTypes = GameTypeOptions.All.Select(o => o.Value).ToList();
        if (!allowedGameTypes.Contains(vm.GameType))
            return "Loại game không hợp lệ.";

        var vocabCount = await _db.Vocabularies!
            .CountAsync(v => v.LessonId == vm.LessonId);

        // Word Scramble chỉ cần 1 từ vựng; Matching cần tối thiểu 2 từ để ghép cặp
        var minVocabRequired = vm.GameType == "Matching" ? 2 : 1;

        if (vocabCount < minVocabRequired)
        {
            return vm.GameType == "Matching"
                ? "Bài học này cần ít nhất 2 từ vựng để tạo trò chơi Ghép từ."
                : "Bài học này chưa có từ vựng. Vui lòng thêm từ vựng trước khi tạo trò chơi.";
        }

        var game = new MiniGame
        {
            Title = vm.Title.Trim(),
            GameType = vm.GameType,
            XPReward = vm.XPReward,
            LessonId = vm.LessonId
        };

        await _gameRepo.AddAsync(game);
        await _gameRepo.SaveChangesAsync();
        return null;
    }

    public async Task<string?> UpdateAsync(EditMiniGameViewModel vm)
    {
        var game = await _db.MiniGames!
            .FirstOrDefaultAsync(g => g.GameId == vm.GameId);

        if (game == null) return "Không tìm thấy trò chơi.";

        game.Title = vm.Title.Trim();
        game.XPReward = vm.XPReward;
        // GameType và LessonId không thay đổi sau khi tạo

        _gameRepo.Update(game);
        await _gameRepo.SaveChangesAsync();
        return null;
    }

    public async Task<string?> DeleteAsync(int gameId)
    {
        var game = await _db.MiniGames!
            .FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null) return "Không tìm thấy trò chơi.";

        var hasProgress = await _db.StudentGameProgresses!
            .AnyAsync(p => p.GameId == gameId);

        if (hasProgress)
            return "Không thể xóa trò chơi này vì đã có học sinh tham gia.";

        _gameRepo.Delete(game);
        await _gameRepo.SaveChangesAsync();
        return null;
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task<List<LessonSelectItem>> GetActiveLessonSelectItemsAsync()
    {
        return await _db.Lessons!
            .AsNoTracking()
            .Where(l => l.IsPublished)
            .OrderBy(l => l.Title)
            .Select(l => new LessonSelectItem { LessonId = l.LessonId, Title = l.Title })
            .ToListAsync();
    }

    private static MiniGameViewModel MapToViewModel(MiniGame g) => new()
    {
        GameId = g.GameId,
        Title = g.Title,
        GameType = g.GameType,
        XPReward = g.XPReward,
        LessonId = g.LessonId,
        LessonTitle = g.Lesson?.Title ?? "—"
    };
}
