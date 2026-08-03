using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IQuizAttemptRepository
{
    Task<List<Quiz>> GetQuizzesByLessonIdAsync(
        int lessonId,
        int studentId,
        int? assignmentId = null);
    Task<Lesson?> GetLessonByIdAsync(int lessonId);
    Task<WeeklyAssignment?> GetAssignmentForLessonAsync(
        int lessonId,
        int studentId,
        int? assignmentId = null);
    Task<Progress?> GetProgressAsync(int studentId, int lessonId);
    Task UpdateProgressAsync(Progress progress);
    Task CreateProgressAsync(Progress progress);
    Task<QuizAttempt> CreateAttemptAsync(QuizAttempt attempt);
    Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId, int studentId);
    Task<List<QuizAttempt>> GetAttemptsByStudentAsync(int studentId, int? lessonId, DateTime? from, DateTime? to, string sort = "date");
    Task<List<Lesson>> GetLessonsWithAttemptsAsync(int studentId);
}
