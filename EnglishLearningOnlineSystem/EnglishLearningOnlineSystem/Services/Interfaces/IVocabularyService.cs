using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

// Interface xử lý nghiệp vụ học từ vựng trong bài học
public interface IVocabularyService
{
    Task<VocabularyViewModel?> GetVocabularyAsync(int lessonId);

    Task<VocabularyHubViewModel> GetAllVocabularyAsync(int studentId);

    // Content Manager
    Task<(List<EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyListItemViewModel> Items, int TotalCount)> GetVocabulariesAsync(string? keyword, int? lessonId, int page, int pageSize);
    Task<(EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyEditViewModel? Model, string? ErrorMessage)> GetVocabularyForEditAsync(int id);
    Task<(bool Success, string? ErrorMessage)> CreateVocabularyAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyCreateViewModel model);
    Task<(bool Success, string? ErrorMessage)> UpdateVocabularyAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyEditViewModel model);
    Task<(bool Success, string? ErrorMessage)> DeleteVocabularyAsync(int id);
}
