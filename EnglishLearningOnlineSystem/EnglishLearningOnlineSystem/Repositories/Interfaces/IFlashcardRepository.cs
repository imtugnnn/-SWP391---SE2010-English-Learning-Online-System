using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IFlashcardRepository
{
    Task<List<Vocabulary>> GetVocabularyByLessonAsync(
        int lessonId,
        int studentId,
        int? assignmentId = null);
    Task<Lesson?> GetLessonByIdAsync(int lessonId);
    Task<FlashcardSession> CreateSessionAsync(FlashcardSession session);
    Task<FlashcardSession?> GetSessionAsync(int sessionId, int studentId);
    Task CompleteSessionAsync(int sessionId, int cardsReviewed, List<FlashcardCardResult> results);
    Task<List<int>> GetMasteredVocabularyIdsAsync(int studentId, int lessonId);
    Task ResetMasteryAsync(int studentId, int lessonId);
}
