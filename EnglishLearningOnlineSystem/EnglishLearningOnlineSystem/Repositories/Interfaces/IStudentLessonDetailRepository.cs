using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

// Interface xử lý dữ liệu chi tiết bài học và tiến độ học tập
public interface IStudentLessonDetailRepository
{
    Task<Lesson?> GetLessonWithContentAsync(int studentId, int lessonId, int? assignmentId = null);
    Task<WeeklyAssignment?> GetAccessibleAssignmentAsync(int studentId, int lessonId, int assignmentId);

    Task<Progress?> GetBestProgressAsync(int studentId, int lessonId);

    Task<int> GetAttemptCountAsync(int studentId, int lessonId, int? assignmentId = null);

    Task<List<StudentGameProgress>> GetGameProgressesAsync(int studentId, int lessonId, int? assignmentId = null);

    Task SaveProgressAsync(int studentId, int lessonId, int score, string answersJson, int xpEarned);
}
