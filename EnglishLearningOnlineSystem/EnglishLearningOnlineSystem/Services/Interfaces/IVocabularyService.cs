using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

// Interface xử lý nghiệp vụ học từ vựng trong bài học
public interface IVocabularyService
{
    Task<VocabularyViewModel?> GetVocabularyAsync(int lessonId);
}