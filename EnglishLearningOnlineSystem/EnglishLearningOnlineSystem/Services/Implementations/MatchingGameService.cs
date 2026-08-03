using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Student.Games;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class MatchingGameService : IMatchingGameService
{
    // Số cặp tối đa mỗi lượt chơi (nếu bài học có nhiều từ vựng hơn, chỉ lấy ngẫu nhiên)
    private const int MaxPairsPerRound = 6;

    // Số cặp tối thiểu cần có để chơi được Matching
    private const int MinPairsRequired = 2;

    private readonly AppDbContext _db;
    private readonly IStudentGameProgressRepository _progressRepo;
    private readonly IAssignmentProgressService _assignmentProgressService;
    private static readonly Random _rng = new();

    public MatchingGameService(
        AppDbContext db,
        IStudentGameProgressRepository progressRepo,
        IAssignmentProgressService assignmentProgressService)
    {
        _db = db;
        _progressRepo = progressRepo;
        _assignmentProgressService = assignmentProgressService;
    }

    public async Task<MatchingPlayViewModel?> LoadPlayAsync(
        int gameId,
        int studentId,
        int? assignmentId = null)
    {
        var game = await _db.MiniGames!
            .AsNoTracking()
            .Include(g => g.Lesson)
            .FirstOrDefaultAsync(g => g.GameId == gameId && g.GameType == "Matching");

        if (game == null) return null;

        var vocabularies = await _db.Vocabularies!
            .AsNoTracking()
            .Where(v => v.LessonId == game.LessonId)
            .ToListAsync();

        if (vocabularies.Count < MinPairsRequired) return null;

        if (assignmentId.HasValue && !await _assignmentProgressService.MarkActivityStartedAsync(
                assignmentId.Value, studentId, AssignmentActivityType.MiniGame, gameId))
            return null;

        var pairCount = Math.Min(MaxPairsPerRound, vocabularies.Count);

        var chosen = vocabularies
            .OrderBy(_ => _rng.Next())
            .Take(pairCount)
            .ToList();

        var words = chosen
            .Select(v => new MatchingItem { VocabularyId = v.VocabularyId, Text = v.Word })
            .OrderBy(_ => _rng.Next())
            .ToList();

        List<MatchingItem> meanings;
        var attempts = 0;
        do
        {
            meanings = chosen
                .Select(v => new MatchingItem { VocabularyId = v.VocabularyId, Text = v.Meaning })
                .OrderBy(_ => _rng.Next())
                .ToList();
            attempts++;
        }
        // Tránh trường hợp xáo trộn xong cột nghĩa vẫn thẳng hàng giống cột từ (chỉ kiểm tra khi >1 cặp)
        while (pairCount > 1
               && words.Select(w => w.VocabularyId).SequenceEqual(meanings.Select(m => m.VocabularyId))
               && attempts < 10);

        return new MatchingPlayViewModel
        {
            GameId = game.GameId,
            AssignmentId = assignmentId,
            GameTitle = game.Title,
            XPReward = game.XPReward,
            LessonId = game.LessonId,
            LessonTitle = game.Lesson?.Title ?? "—",
            Words = words,
            Meanings = meanings
        };
    }

    public async Task<(MatchingResultViewModel? Result, string? Error)> SubmitAsync(
        MatchingSubmitViewModel vm,
        int studentId)
    {
        var game = await _db.MiniGames!
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.GameId == vm.GameId && g.GameType == "Matching");

        if (game == null)
            return (null, "Không tìm thấy trò chơi.");
        if (vm.AssignmentId.HasValue && !await _assignmentProgressService.MarkActivityStartedAsync(
                vm.AssignmentId.Value, studentId, AssignmentActivityType.MiniGame, vm.GameId))
            return (null, "Trò chơi không thuộc bài giao của bạn.");

        if (vm.Answers == null || vm.Answers.Count == 0)
            return (null, "Vui lòng ghép ít nhất một cặp từ.");

        var vocabIds = vm.Answers.Select(a => a.VocabularyId)
            .Concat(vm.Answers.Select(a => a.SelectedMeaningVocabularyId))
            .Distinct()
            .ToList();

        var vocabularies = await _db.Vocabularies!
            .AsNoTracking()
            .Where(v => vocabIds.Contains(v.VocabularyId))
            .ToDictionaryAsync(v => v.VocabularyId);

        var studentProfile = await _db.StudentProfiles!
            .FirstOrDefaultAsync(s => s.StudentId == studentId);

        if (studentProfile == null)
            return (null, "Không tìm thấy hồ sơ học sinh.");

        var items = new List<MatchingResultItem>();
        var correctCount = 0;

        foreach (var answer in vm.Answers)
        {
            if (!vocabularies.TryGetValue(answer.VocabularyId, out var wordVocab))
                continue; // Bỏ qua dữ liệu submit không hợp lệ (từ vựng không thuộc bài học)

            var isCorrect = answer.SelectedMeaningVocabularyId == answer.VocabularyId;
            if (isCorrect) correctCount++;

            var yourMeaning = vocabularies.TryGetValue(answer.SelectedMeaningVocabularyId, out var selectedVocab)
                ? selectedVocab.Meaning
                : "(chưa chọn)";

            items.Add(new MatchingResultItem
            {
                Word = wordVocab.Word,
                YourMeaning = yourMeaning,
                CorrectMeaning = wordVocab.Meaning,
                IsCorrect = isCorrect
            });
        }

        var totalCount = items.Count;

        // XP được tính theo tỉ lệ số cặp ghép đúng (chia nguyên) trên tổng XP của game
        var xpEarned = totalCount > 0 ? (game.XPReward * correctCount) / totalCount : 0;

        var progress = new StudentGameProgress
        {
            StudentId = studentId,
            GameId = game.GameId,
            WeeklyAssignmentId = vm.AssignmentId,
            Score = totalCount > 0 ? (correctCount * 100) / totalCount : 0,
            XPEarned = xpEarned,
            CompletedAt = DateTime.UtcNow
        };
        await _progressRepo.AddAsync(progress);

        if (xpEarned > 0)
        {
            studentProfile.XP += xpEarned;

            var xpTransaction = new XpTransaction
            {
                StudentId = studentId,
                Amount = xpEarned,
                Source = "MiniGame",
                CreatedAt = DateTime.UtcNow
            };
            await _db.XpTransactions!.AddAsync(xpTransaction);
        }

        await _progressRepo.SaveChangesAsync();
        if (vm.AssignmentId.HasValue)
        {
            await _assignmentProgressService.MarkActivityCompletedAsync(
                vm.AssignmentId.Value,
                studentId,
                AssignmentActivityType.MiniGame,
                vm.GameId,
                progress.Score);
        }

        return (new MatchingResultViewModel
        {
            GameId = game.GameId,
            AssignmentId = vm.AssignmentId,
            GameTitle = game.Title,
            LessonId = game.LessonId,
            Items = items,
            CorrectCount = correctCount,
            TotalCount = totalCount,
            XPEarned = xpEarned,
            NewTotalXP = studentProfile.XP
        }, null);
    }
}
