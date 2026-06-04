using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

// Interface xử lý dữ liệu từ vựng trong bài học
public interface IVocabularyRepository
{
    Task<Lesson?> GetLessonWithVocabAsync(int lessonId);
}