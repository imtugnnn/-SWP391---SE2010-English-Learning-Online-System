using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;

    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Lesson>> GetPublishedLessonsByCourseIdAsync(int courseId)
    {
        return await _context.Lessons!
            .Include(l => l.Vocabularies)
            .Include(l => l.Quizzes)
            .Include(l => l.MiniGames)
            .Where(l => l.CourseId == courseId && l.IsPublished)
            .AsNoTracking()
            .AsSplitQuery()
            .OrderBy(l => l.OrderIndex)
            .ToListAsync();
    }

    public async Task<Lesson?> GetPublishedLessonDetailAsync(int courseId, int lessonId)
    {
        return await _context.Lessons!
            .Include(l => l.Vocabularies)
            .Include(l => l.Quizzes)
            .Include(l => l.MiniGames)
            .AsNoTracking()
            .AsSplitQuery()
            .FirstOrDefaultAsync(l =>
                l.LessonId == lessonId &&
                l.CourseId == courseId &&
                l.IsPublished);
    }

    public async Task<List<int>> GetAssignedLessonIdsAsync(
        int courseId,
        List<int> lessonIds,
        DateTime weekStartDate)
    {
        return await _context.WeeklyAssignments!
            .Where(a =>
                a.CourseId == courseId &&
                a.LessonId.HasValue &&
                lessonIds.Contains(a.LessonId.Value) &&
                a.WeekStartDate.Date == weekStartDate.Date)
            .Select(a => a.LessonId!.Value)
            .ToListAsync();
    }

    public async Task AddWeeklyAssignmentsAsync(List<WeeklyAssignment> assignments)
    {
        await _context.WeeklyAssignments!.AddRangeAsync(assignments);
    }

    public async Task<bool> ValidateLessonsBelongToCourseAsync(int courseId, List<int> lessonIds)
    {
        var distinctLessonIds = lessonIds.Distinct().ToList();
        var matchingLessonCount = await _context.Lessons!
            .CountAsync(l => distinctLessonIds.Contains(l.LessonId) &&
                             l.CourseId == courseId &&
                             l.IsPublished);

        return matchingLessonCount == distinctLessonIds.Count;
    }
    public async Task<List<WeeklyAssignment>> GetAssignmentsByCourseIdsAsync(List<int> courseIds)
    {
        if (!courseIds.Any())
        {
            return new List<WeeklyAssignment>();
        }

        return await _context.WeeklyAssignments!
            .Include(a => a.Lesson)
                .ThenInclude(l => l.Vocabularies)
            .Include(a => a.Lesson)
                .ThenInclude(l => l.Quizzes)
            .Include(a => a.Lesson)
                .ThenInclude(l => l.MiniGames)
            .Where(a => a.CourseId.HasValue && courseIds.Contains(a.CourseId.Value))
            .AsNoTracking()
            .AsSplitQuery()
            .OrderByDescending(a => a.DueDate)
            .ToListAsync();
    }
    public async Task<List<Course>> GetPublishedCoursesAsync()
    {
        return await _context.Courses!
            .Where(c => c.IsPublished && !c.IsDeleted)
            .AsNoTracking()
            .OrderBy(c => c.GradeLevel)
            .ThenBy(c => c.CourseName)
            .ToListAsync();
    }
}
