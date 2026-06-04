using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class FlashcardRepository : IFlashcardRepository
{
    private readonly AppDbContext _db;

    public FlashcardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Vocabulary>> GetVocabularyByLessonAsync(int lessonId)
    {
        return await _db.Vocabularies!
            .Where(v => v.LessonId == lessonId)
            .OrderBy(v => v.VocabularyId)
            .ToListAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(int lessonId)
    {
        return await _db.Lessons!
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);
    }

    public async Task<FlashcardSession> CreateSessionAsync(FlashcardSession session)
    {
        _db.FlashcardSessions.Add(session);
        await _db.SaveChangesAsync();
        return session;
    }

    public async Task<FlashcardSession?> GetSessionAsync(int sessionId, int studentId)
    {
        return await _db.FlashcardSessions
            .Include(s => s.Lesson)
            .Include(s => s.CardResults)
                .ThenInclude(r => r.Vocabulary)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.StudentId == studentId);
    }

    public async Task CompleteSessionAsync(int sessionId, int cardsReviewed, List<FlashcardCardResult> results)
    {
        var session = await _db.FlashcardSessions.FindAsync(sessionId);
        if (session == null) return;

        session.CardsReviewed = cardsReviewed;
        session.CompletedAt = DateTime.UtcNow;

        if (results.Any())
        {
            _db.FlashcardCardResults.AddRange(results);
        }

        await _db.SaveChangesAsync();
    }
}
