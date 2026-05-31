using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IAdaptiveLearningService
{
    Task<List<LessonRecommendation>> GetSuggestionsAsync(int studentId);
}
