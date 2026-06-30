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
        var attempts = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId)
            .Select(qa => qa.Score)
            .ToListAsync();

        if (!attempts.Any()) return 0;

        // Score already stored as percentage (0–100)
        return Math.Round(attempts.Average(), 1);
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

    public async Task<int> GetTotalXpAwardedAsync(int lessonId)
    {
        var attempts = await _db.QuizAttempts
            .AsNoTracking()
            .Include(qa => qa.Lesson)
            .Where(qa => qa.LessonId == lessonId && qa.XpAwarded)
            .ToListAsync();

        return attempts.Sum(a => a.Lesson?.XPReward ?? 0);
    }

    public async Task<double> GetAverageStudyMinutesAsync(int lessonId)
    {
        var attempts = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => qa.LessonId == lessonId && qa.TimeSpentSec > 0)
            .Select(qa => qa.TimeSpentSec)
            .ToListAsync();

        if (!attempts.Any())
        {
            // fallback to lesson estimated time
            var lesson = await _db.Lessons!
                .AsNoTracking()
                .Where(l => l.LessonId == lessonId)
                .Select(l => l.EstimatedMinutes)
                .FirstOrDefaultAsync();

            return lesson;
        }

        return Math.Round(attempts.Average() / 60.0, 1);
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

    public async Task<IEnumerable<LessonAnalyticsRowData>> GetAllLessonsSummaryAsync(int? courseId = null)
    {
        var lessonsQuery = _db.Lessons!
            .AsNoTracking()
            .Include(l => l.Course)
            .Where(l => l.Course != null && !l.Course.IsDeleted);

        if (courseId.HasValue)
            lessonsQuery = lessonsQuery.Where(l => l.CourseId == courseId.Value);

        var lessons = await lessonsQuery
            .OrderBy(l => l.CourseId)
            .ThenBy(l => l.OrderIndex)
            .ToListAsync();

        if (!lessons.Any())
            return Enumerable.Empty<LessonAnalyticsRowData>();

        var lessonIds = lessons.Select(l => l.LessonId).ToList();

        // Total distinct students per lesson
        var studentCounts = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => lessonIds.Contains(qa.LessonId))
            .GroupBy(qa => qa.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                Count = g.Select(qa => qa.StudentId).Distinct().Count()
            })
            .ToDictionaryAsync(x => x.LessonId, x => x.Count);

        // Average score per lesson
        var avgScores = await _db.QuizAttempts
            .AsNoTracking()
            .Where(qa => lessonIds.Contains(qa.LessonId))
            .GroupBy(qa => qa.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                AvgPct = g.Average(qa => qa.Score)
            })
            .ToDictionaryAsync(x => x.LessonId, x => Math.Round(x.AvgPct, 1));

        // Total XP awarded per lesson
        var xpTotals = await _db.QuizAttempts
            .AsNoTracking()
            .Include(qa => qa.Lesson)
            .Where(qa => lessonIds.Contains(qa.LessonId) && qa.XpAwarded)
            .GroupBy(qa => qa.LessonId)
            .Select(g => new
            {
                LessonId = g.Key,
                Total = g.Sum(qa => qa.Lesson!.XPReward)
            })
            .ToDictionaryAsync(x => x.LessonId, x => x.Total);

        // Flashcard completion rates
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

                    int done = list.Count(s =>
                        s.CardResults.Any() &&
                        s.CardResults.All(cr => cr.KnewIt));

                    return Math.Round((double)done / list.Count * 100, 1);
                });

        return lessons.Select(l => new LessonAnalyticsRowData(
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
        ));
    }
}