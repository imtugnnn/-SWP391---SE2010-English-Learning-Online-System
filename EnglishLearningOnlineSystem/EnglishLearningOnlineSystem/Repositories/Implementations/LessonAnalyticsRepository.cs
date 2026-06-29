using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class LessonAnalyticsRepository : ILessonAnalyticsRepository
{
    private readonly AppDbContext _db;

    public LessonAnalyticsRepository(AppDbContext db)
    {
        _db = db;
    }

    // ── Per-lesson ─────────────────────────────────────────────────────────────

    public async Task<LessonCoreStatsData> GetLessonCoreStatsAsync(int lessonId)
    {
        var stats = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId)
            .GroupBy(qa => qa.LessonId)
            .Select(g => new
            {
                TotalStudents = g.Select(qa => qa.StudentId).Distinct().Count(),
                AvgScore = g.Average(qa => qa.Score),
                TotalAttempts = g.Count(),
                TotalXp = g.Where(qa => qa.XpAwarded).Sum(qa => qa.Lesson!.XPReward)
            })
            .FirstOrDefaultAsync();

        return new LessonCoreStatsData(
            TotalStudents: stats?.TotalStudents ?? 0,
            AvgQuizScore: stats == null ? 0 : Math.Round(stats.AvgScore, 1),
            TotalXpAwarded: stats?.TotalXp ?? 0,
            TotalQuizAttempts: stats?.TotalAttempts ?? 0
        );
    }

    public async Task<int> GetTotalStudentsAsync(int lessonId)
    {
        return await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId)
            .Select(qa => qa.StudentId)
            .Distinct()
            .CountAsync();
    }

    public async Task<double> GetAverageQuizScoreAsync(int lessonId)
    {
        var hasAny = await _db.QuizAttempts.AsNoTracking().AnyAsync(qa => qa.LessonId == lessonId);
        if (!hasAny) return 0;

        var avg = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId)
            .AverageAsync(qa => qa.Score);

        return Math.Round(avg, 1);
    }

    public async Task<double> GetFlashcardCompletionRateAsync(int lessonId)
    {
        var sessions = await _db.FlashcardSessions
            .AsNoTracking()
            .Include(fs => fs.CardResults)
            .Where(fs => fs.LessonId == lessonId)
            .ToListAsync();

        if (!sessions.Any()) return 0;

        int completed = sessions.Count(s =>
            s.CardResults.Any() &&
            s.CardResults.All(cr => cr.KnewIt));

        return Math.Round((double)completed / sessions.Count * 100, 1);
    }

    public async Task<double> GetFlashcardAccuracyRateAsync(int lessonId)
    {
        var sessions = await _db.FlashcardSessions
            .AsNoTracking()
            .Include(fs => fs.CardResults)
            .Where(fs => fs.LessonId == lessonId)
            .ToListAsync();

        var allResults = sessions.SelectMany(s => s.CardResults).ToList();
        if (!allResults.Any()) return 0;

        return Math.Round(allResults.Count(cr => cr.KnewIt) / (double)allResults.Count * 100, 1);
    }

    public async Task<int> GetTotalXpAwardedAsync(int lessonId)
    {
        // Tính trực tiếp trong SQL (JOIN + SUM) thay vì kéo cả list về rồi Sum() trong C#.
        return await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId && qa.XpAwarded)
            .SumAsync(qa => qa.Lesson!.XPReward);
    }

    public async Task<double> GetAverageStudyMinutesAsync(int lessonId)
    {
        var timed = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId && qa.TimeSpentSec > 0)
            .Select(qa => qa.TimeSpentSec)
            .ToListAsync();

        if (!timed.Any())
        {
            var lesson = await _db.Lessons!
                .AsNoTracking()
                .Where(l => l.LessonId == lessonId)
                .Select(l => l.EstimatedMinutes)
                .FirstOrDefaultAsync();

            return lesson;
        }

        return Math.Round(timed.Average() / 60.0, 1);
    }

    public async Task<Dictionary<string, int>> GetDailyAttemptCountsAsync(int lessonId, int days = 30)
    {
        var since = DateTime.UtcNow.AddDays(-days).Date;

        var raw = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId && qa.SubmittedAt >= since)
            .GroupBy(qa => qa.SubmittedAt.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<string, int>();

        for (int i = days - 1; i >= 0; i--)
        {
            var day = DateTime.UtcNow.AddDays(-i).Date;
            var key = day.ToString("MMM dd");

            var match = raw.FirstOrDefault(r => r.Date == day);
            result[key] = match?.Count ?? 0;
        }

        return result;
    }

    public async Task<Dictionary<string, int>> GetScoreDistributionAsync(int lessonId)
    {
        var attempts = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId)
            .Select(qa => qa.Score)
            .ToListAsync();

        var buckets = new Dictionary<string, int>
        {
            ["0–49"] = 0,
            ["50–69"] = 0,
            ["70–84"] = 0,
            ["85–100"] = 0
        };

        foreach (var score in attempts)
        {
            if (score < 50) buckets["0–49"]++;
            else if (score < 70) buckets["50–69"]++;
            else if (score < 85) buckets["70–84"]++;
            else buckets["85–100"]++;
        }

        return buckets;
    }

    // ── Cross-lesson dashboard ─────────────────────────────────────────────────

    public async Task<IEnumerable<LessonAnalyticsRowData>> GetAllLessonsSummaryAsync(
        int? courseId = null,
        string? search = null,
        string? sortBy = null)
    {
        var lessonsQuery = _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .Where(l => l.Course != null && !l.Course.IsDeleted);

        if (courseId.HasValue)
            lessonsQuery = lessonsQuery.Where(l => l.CourseId == courseId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            lessonsQuery = lessonsQuery.Where(l => l.Title.Contains(term));
        }

        var lessons = await lessonsQuery
            .OrderBy(l => l.CourseId)
            .ThenBy(l => l.OrderIndex)
            .ToListAsync();

        if (!lessons.Any())
            return Enumerable.Empty<LessonAnalyticsRowData>();

        var lessonIds = lessons.Select(l => l.LessonId).ToList();

        var studentCounts = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => lessonIds.Contains(qa.LessonId))
            .GroupBy(qa => qa.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Select(qa => qa.StudentId).Distinct().Count() })
            .ToDictionaryAsync(x => x.LessonId, x => x.Count);

        var avgScores = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => lessonIds.Contains(qa.LessonId))
            .GroupBy(qa => qa.LessonId)
            .Select(g => new { LessonId = g.Key, AvgPct = g.Average(qa => qa.Score) })
            .ToDictionaryAsync(x => x.LessonId, x => Math.Round(x.AvgPct, 1));

        var xpTotals = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => lessonIds.Contains(qa.LessonId) && qa.XpAwarded)
            .GroupBy(qa => qa.LessonId)
            .Select(g => new { LessonId = g.Key, Total = g.Sum(qa => qa.Lesson!.XPReward) })
            .ToDictionaryAsync(x => x.LessonId, x => x.Total);

        var sessions = await _db.FlashcardSessions
            .AsNoTracking()
            .Include(fs => fs.CardResults)
            .Where(fs => lessonIds.Contains(fs.LessonId))
            .ToListAsync();

        var flashRates = sessions
            .GroupBy(fs => fs.LessonId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var list = g.ToList();
                    int done = list.Count(s => s.CardResults.Any() && s.CardResults.All(cr => cr.KnewIt));
                    return Math.Round((double)done / list.Count * 100, 1);
                });

        var result = lessons.Select(l => new LessonAnalyticsRowData(
            LessonId: l.LessonId,
            Title: l.Title,
            CourseName: l.Course?.CourseName ?? "—",
            CourseId: l.CourseId,
            IsPublished: l.IsPublished,
            EstimatedMinutes: l.EstimatedMinutes,
            TotalStudents: studentCounts.GetValueOrDefault(l.LessonId, 0),
            AvgQuizScore: avgScores.GetValueOrDefault(l.LessonId, 0),
            FlashcardCompletionRate: flashRates.GetValueOrDefault(l.LessonId, 0),
            TotalXpAwarded: xpTotals.GetValueOrDefault(l.LessonId, 0)
        )).ToList();

        return sortBy switch
        {
            "students_desc" => result.OrderByDescending(r => r.TotalStudents),
            "score_desc" => result.OrderByDescending(r => r.AvgQuizScore),
            "score_asc" => result.OrderBy(r => r.AvgQuizScore),
            "xp_desc" => result.OrderByDescending(r => r.TotalXpAwarded),
            "title_asc" => result.OrderBy(r => r.Title, StringComparer.OrdinalIgnoreCase),
            _ => result
        };
    }

    public async Task<(double WeightedAvgScore, int UniqueStudents)> GetOverallStatsAsync(IEnumerable<int> lessonIds)
    {
        var ids = lessonIds.ToList();
        if (!ids.Any()) return (0, 0);

        var avgScore = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => ids.Contains(qa.LessonId))
            .Select(qa => (double?)qa.Score)
            .AverageAsync() ?? 0;

        var uniqueStudents = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => ids.Contains(qa.LessonId))
            .Select(qa => qa.StudentId)
            .Distinct()
            .CountAsync();

        return (Math.Round(avgScore, 1), uniqueStudents);
    }
}