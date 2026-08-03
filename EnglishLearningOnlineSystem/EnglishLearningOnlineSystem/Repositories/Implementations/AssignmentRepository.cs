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
        int classId,
        int courseId,
        List<int> lessonIds,
        DateTime weekStartDate)
    {
        return await _context.WeeklyAssignments!
            .Where(a =>
                a.ClassId == classId &&
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

    public async Task<int> CountPublishedLessonsAsync(int courseId, List<int> lessonIds)
    {
        var distinctLessonIds = lessonIds.Distinct().ToList();
        return await _context.Lessons!
            .CountAsync(l => distinctLessonIds.Contains(l.LessonId) &&
                             l.CourseId == courseId &&
                             l.IsPublished);
    }
    public async Task<List<WeeklyAssignment>> GetAssignmentsByClassIdsAsync(List<int> classIds)
    {
        if (!classIds.Any())
        {
            return new List<WeeklyAssignment>();
        }

        return await _context.WeeklyAssignments!
            .Include(a => a.Class)
                .ThenInclude(c => c!.Enrollments)
                    .ThenInclude(e => e.Student)
            .Include(a => a.Lesson)
                .ThenInclude(l => l.Vocabularies)
            .Include(a => a.Lesson)
                .ThenInclude(l => l.Quizzes)
            .Include(a => a.Lesson)
                .ThenInclude(l => l.MiniGames)
            .Include(a => a.Vocabularies)
            .Include(a => a.Quizzes)
            .Include(a => a.MiniGames)
            .Include(a => a.StudentProgresses)
            .Where(a => a.ClassId.HasValue && classIds.Contains(a.ClassId.Value))
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

    public async Task<WeeklyAssignment?> GetForUpdateAsync(
        int assignmentId,
        int classId,
        int courseId)
    {
        return await _context.WeeklyAssignments!
            .FirstOrDefaultAsync(assignment =>
                assignment.AssignmentId == assignmentId &&
                assignment.ClassId == classId &&
                assignment.CourseId == courseId);
    }

    public async Task<bool> ExistsPublishedAssignmentAsync(
        int classId,
        int courseId,
        int? lessonId,
        DateTime weekStartDate,
        int excludedAssignmentId)
    {
        return await _context.WeeklyAssignments!
            .AnyAsync(candidate =>
                candidate.AssignmentId != excludedAssignmentId &&
                candidate.ClassId == classId &&
                candidate.CourseId == courseId &&
                candidate.LessonId == lessonId &&
                candidate.WeekStartDate.Date == weekStartDate.Date &&
                candidate.IsVisible);
    }

    public Task<WeeklyAssignment?> GetAssignmentDetailsAsync(int assignmentId, int classId)
    {
        return _context.WeeklyAssignments!
            .Include(x => x.Class)
                .ThenInclude(x => x!.Enrollments)
                    .ThenInclude(x => x.Student)
            .Include(x => x.Course)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Vocabularies)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.Quizzes)
            .Include(x => x.Lesson)
                .ThenInclude(x => x.MiniGames)
            .Include(x => x.Vocabularies)
            .Include(x => x.Quizzes)
            .Include(x => x.MiniGames)
            .Include(x => x.StudentProgresses)
                .ThenInclude(x => x.Student)
                    .ThenInclude(x => x.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.ClassId == classId);
    }

    public Task<bool> HasStudentProgressAsync(int assignmentId)
    {
        return _context.AssignmentProgresses.AnyAsync(x => x.AssignmentId == assignmentId);
    }

    public void RemoveAssignment(WeeklyAssignment assignment)
    {
        _context.WeeklyAssignments!.Remove(assignment);
    }
}
