using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IQuizAttemptService
{
    Task<TakeQuizViewModel?> GetQuizForLessonAsync(int lessonId, int studentId);
    Task<QuizResultViewModel?> SubmitQuizAsync(int studentId, QuizSubmitViewModel submitData);
    Task<QuizResultViewModel?> GetAttemptResultAsync(int attemptId, int studentId);
    Task<AttemptHistoryViewModel> GetStudentHistoryAsync(int studentId, int? lessonId, string? from, string? to, string sort);
    Task<ReviewIncorrectViewModel?> GetIncorrectAnswersAsync(int attemptId, int studentId, bool showAll = false);
}
