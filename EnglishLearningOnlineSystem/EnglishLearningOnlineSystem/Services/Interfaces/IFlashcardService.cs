using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IFlashcardService
{
    Task<FlashcardPracticeViewModel?> StartSessionAsync(
        int lessonId,
        int studentId,
        bool resetProgress = false,
        int? assignmentId = null);
    Task CompleteSessionAsync(int studentId, FlashcardCompleteViewModel completeData);
    Task<FlashcardResultViewModel?> GetSessionResultAsync(int sessionId, int studentId);
}
