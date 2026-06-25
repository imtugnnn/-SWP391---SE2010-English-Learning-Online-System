using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

// Interface xử lý dữ liệu từ vựng trong bài học
public interface IVocabularyRepository
{
    Task<Lesson?> GetLessonWithVocabAsync(int lessonId);

    // Lấy tất cả từ vựng từ các bài học được giao cho học sinh
    Task<List<Vocabulary>> GetAllVocabByStudentAsync(int studentId);

    // Content Manager CRUD
    Task<(List<Vocabulary> Items, int TotalCount)> GetVocabulariesPaginatedAsync(string? keyword, int? lessonId, int page, int pageSize);
    Task<Vocabulary?> GetVocabularyByIdAsync(int id);
    Task AddVocabularyAsync(Vocabulary vocabulary);
    Task UpdateVocabularyAsync(Vocabulary vocabulary);
    Task DeleteVocabularyAsync(Vocabulary vocabulary);
}
