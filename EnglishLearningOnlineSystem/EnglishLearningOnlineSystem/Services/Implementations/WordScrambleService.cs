using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Student.Games;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class WordScrambleService : IWordScrambleService
{
    private readonly AppDbContext _db;
    private readonly IStudentGameProgressRepository _progressRepo;
    private readonly IAssignmentProgressService _assignmentProgressService;
    private static readonly Random _rng = new();

    public WordScrambleService(
        AppDbContext db,
        IStudentGameProgressRepository progressRepo,
        IAssignmentProgressService assignmentProgressService)
    {
        _db = db;
        _progressRepo = progressRepo;
        _assignmentProgressService = assignmentProgressService;
    }

    public async Task<StudentMiniGameListViewModel?> GetGamesByLessonAsync(int lessonId)
    {
        var lesson = await _db.Lessons!
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.IsPublished);

        if (lesson == null) return null;

        var games = await _db.MiniGames!
            .AsNoTracking()
            .Where(g => g.LessonId == lessonId && g.GameType == "WordScramble")
            .OrderBy(g => g.Title)
            .Select(g => new StudentMiniGameItemViewModel
            {
                GameId   = g.GameId,
                Title    = g.Title,
                XPReward = g.XPReward
            })
            .ToListAsync();

        return new StudentMiniGameListViewModel
        {
            LessonId    = lesson.LessonId,
            LessonTitle = lesson.Title,
            Games       = games
        };
    }

    public async Task<WordScramblePlayViewModel?> LoadPlayAsync(
        int gameId,
        int studentId,
        int? assignmentId = null)
    {
        var game = await _db.MiniGames!
            .AsNoTracking()
            .Include(g => g.Lesson)
            .FirstOrDefaultAsync(g => g.GameId == gameId && g.GameType == "WordScramble");

        if (game == null) return null;

        var vocabularies = await _db.Vocabularies!
            .AsNoTracking()
            .Where(v => v.LessonId == game.LessonId)
            .ToListAsync();

        if (!vocabularies.Any()) return null;

        if (assignmentId.HasValue && !await _assignmentProgressService.MarkActivityStartedAsync(
                assignmentId.Value, studentId, AssignmentActivityType.MiniGame, gameId))
            return null;

        // Chọn ngẫu nhiên một từ vựng
        var vocab = vocabularies[_rng.Next(vocabularies.Count)];

        return new WordScramblePlayViewModel
        {
            GameId       = game.GameId,
            AssignmentId = assignmentId,
            GameTitle    = game.Title,
            XPReward     = game.XPReward,
            LessonId     = game.LessonId,
            LessonTitle  = game.Lesson?.Title ?? "—",
            VocabularyId = vocab.VocabularyId,
            ScrambledWord = ScrambleWord(vocab.Word),
            Meaning      = vocab.Meaning,
            ImageUrl     = vocab.ImageUrl,
            AudioUrl     = vocab.AudioUrl
        };
    }

    public async Task<(WordScrambleResultViewModel? Result, string? Error)> SubmitAsync(
        WordScrambleSubmitViewModel vm,
        int studentId)
    {
        var game = await _db.MiniGames!
            .AsNoTracking()
            .Include(g => g.Lesson)
            .FirstOrDefaultAsync(g => g.GameId == vm.GameId);

        if (game == null)
            return (null, "Không tìm thấy trò chơi.");
        if (vm.AssignmentId.HasValue && !await _assignmentProgressService.MarkActivityStartedAsync(
                vm.AssignmentId.Value, studentId, AssignmentActivityType.MiniGame, vm.GameId))
            return (null, "Trò chơi không thuộc bài giao của bạn.");

        var vocab = await _db.Vocabularies!
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.VocabularyId == vm.VocabularyId);

        if (vocab == null)
            return (null, "Không tìm thấy từ vựng.");

        var studentProfile = await _db.StudentProfiles!
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (studentProfile == null)
            return (null, "Không tìm thấy hồ sơ học sinh.");

        // Kiểm tra đáp án (bỏ qua hoa/thường và khoảng trắng đầu cuối)
        var isCorrect = string.Equals(
            vm.Answer.Trim(),
            vocab.Word.Trim(),
            StringComparison.OrdinalIgnoreCase);

        var xpEarned = isCorrect ? game.XPReward : 0;

        // Lưu tiến trình
        var progress = new StudentGameProgress
        {
            StudentId   = studentId,
            GameId      = game.GameId,
            WeeklyAssignmentId = vm.AssignmentId,
            Score       = isCorrect ? 100 : 0,
            XPEarned    = xpEarned,
            CompletedAt = DateTime.UtcNow
        };
        await _progressRepo.AddAsync(progress);

        // Cập nhật XP học sinh
        if (xpEarned > 0)
        {
            studentProfile.XP += xpEarned;

            // Ghi XpTransaction
            var xpTransaction = new XpTransaction
            {
                StudentId = studentId,
                Amount    = xpEarned,
                Source    = "MiniGame",
                CreatedAt = DateTime.UtcNow
            };
            await _db.XpTransactions!.AddAsync(xpTransaction);
        }

        await _progressRepo.SaveChangesAsync();
        if (vm.AssignmentId.HasValue)
        {
            // Business process: mỗi mini-game được cấu hình là một activity bắt buộc độc lập.
            await _assignmentProgressService.MarkActivityCompletedAsync(
                vm.AssignmentId.Value,
                studentId,
                AssignmentActivityType.MiniGame,
                vm.GameId,
                progress.Score);
        }

        var result = new WordScrambleResultViewModel
        {
            GameId        = game.GameId,
            AssignmentId  = vm.AssignmentId,
            GameTitle     = game.Title,
            IsCorrect     = isCorrect,
            CorrectWord   = vocab.Word,
            StudentAnswer = vm.Answer.Trim(),
            XPEarned      = xpEarned,
            NewTotalXP    = studentProfile.XP,
            LessonId      = game.LessonId
        };

        return (result, null);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    /// <summary>Xáo trộn các chữ cái trong từ. Đảm bảo kết quả khác từ gốc.</summary>
    private static string ScrambleWord(string word)
    {
        if (word.Length <= 1) return word;

        var chars = word.ToCharArray();
        var attempts = 0;

        do
        {
            // Fisher-Yates shuffle
            for (var i = chars.Length - 1; i > 0; i--)
            {
                var j = _rng.Next(i + 1);
                (chars[i], chars[j]) = (chars[j], chars[i]);
            }
            attempts++;
        }
        // Đảm bảo từ bị xáo trộn khác với từ gốc (tối đa 10 lần thử)
        while (new string(chars).Equals(word, StringComparison.OrdinalIgnoreCase)
               && attempts < 10);

        return new string(chars);
    }
}
