using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class QuizAttemptRepository : IQuizAttemptRepository
{
    private readonly AppDbContext _db;

    public QuizAttemptRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Quiz>> GetQuizzesByLessonIdAsync(
        int lessonId,
        int studentId,
        int? assignmentId = null)
    {
        var assignment = await GetAssignmentForLessonAsync(
            lessonId,
            studentId,
            assignmentId);
        if (assignment?.IncludeQuiz == true)
        {
            var selectedIds = await _db.WeeklyAssignmentQuizzes
                .Where(x => x.AssignmentId == assignment.AssignmentId)
                .Select(x => x.QuizId)
                .ToListAsync();

            return await _db.Quizzes!
                .Where(q => q.LessonId == lessonId && selectedIds.Contains(q.QuizId))
                .OrderBy(q => q.QuizId)
                .ToListAsync();
        }

        if (assignment != null)
        {
            return new List<Quiz>();
        }

        return await _db.Quizzes!
            .Where(q => q.LessonId == lessonId)
            .OrderBy(q => q.QuizId)
            .ToListAsync();
    }

    public async Task<Lesson?> GetLessonByIdAsync(int lessonId)
    {
        return await _db.Lessons!
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId);
    }

    public async Task<WeeklyAssignment?> GetAssignmentForLessonAsync(
        int lessonId,
        int studentId,
        int? assignmentId = null)
    {
        return await _db.WeeklyAssignments!
            .Where(wa =>
                wa.LessonId == lessonId &&
                wa.IsVisible &&
                (!assignmentId.HasValue || wa.AssignmentId == assignmentId.Value) &&
                wa.ClassId.HasValue &&
                _db.ClassEnrollments!.Any(e =>
                    e.ClassId == wa.ClassId.Value && e.StudentId == studentId))
            .OrderByDescending(wa => wa.DueDate)
            .FirstOrDefaultAsync();
    }

    public async Task<Progress?> GetProgressAsync(int studentId, int lessonId)
    {
        return await _db.Progresses!
            .FirstOrDefaultAsync(p => p.StudentId == studentId && p.LessonId == lessonId);
    }

    public async Task UpdateProgressAsync(Progress progress)
    {
        _db.Progresses!.Update(progress);
        await _db.SaveChangesAsync();
    }

    public async Task CreateProgressAsync(Progress progress)
    {
        _db.Progresses!.Add(progress);
        await _db.SaveChangesAsync();
    }

    public async Task<QuizAttempt> CreateAttemptAsync(QuizAttempt attempt)
    {
        _db.QuizAttempts.Add(attempt);
        await _db.SaveChangesAsync();
        return attempt;
    }

    public async Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId, int studentId)
    {
        return await _db.QuizAttempts
            .Include(a => a.Answers)
                .ThenInclude(ans => ans.Quiz)
            .Include(a => a.Lesson)
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId && a.StudentId == studentId);
    }

    public async Task<List<QuizAttempt>> GetAttemptsByStudentAsync(
        int studentId, int? lessonId, DateTime? from, DateTime? to, string sort = "date")
    {
        var query = _db.QuizAttempts
            .Include(a => a.Lesson)
            .Where(a => a.StudentId == studentId);

        if (lessonId.HasValue)
            query = query.Where(a => a.LessonId == lessonId.Value);

        if (from.HasValue)
            query = query.Where(a => a.SubmittedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.SubmittedAt <= to.Value.Date.AddDays(1));

        query = sort switch
        {
            "score" => query.OrderByDescending(a => a.Score),
            "score_asc" => query.OrderBy(a => a.Score),
            _ => query.OrderByDescending(a => a.SubmittedAt)
        };

        return await query.ToListAsync();
    }

    public async Task<List<Lesson>> GetLessonsWithAttemptsAsync(int studentId)
    {
        var lessonIds = await _db.QuizAttempts
            .Where(a => a.StudentId == studentId)
            .Select(a => a.LessonId)
            .Distinct()
            .ToListAsync();

        return await _db.Lessons!
            .Where(l => lessonIds.Contains(l.LessonId))
            .OrderBy(l => l.Title)
            .ToListAsync();
    }
}
