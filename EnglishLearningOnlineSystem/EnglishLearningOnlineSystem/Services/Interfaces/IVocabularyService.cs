using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

// Interface xử lý nghiệp vụ học từ vựng trong bài học
public interface IVocabularyService
{
    Task<VocabularyViewModel?> GetVocabularyAsync(int lessonId);

    // Lấy tất cả từ vựng của học sinh gom nhóm theo bài học
    Task<VocabularyHubViewModel> GetAllVocabularyAsync(int studentId);
}
