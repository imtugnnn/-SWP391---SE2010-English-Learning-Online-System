using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class ParentStudentLinkRepository : IParentStudentLinkRepository
{
    private readonly AppDbContext _context;

    public ParentStudentLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<ParentStudentLink>> GetByParentIdAsync(int parentId)
    {
        return _context.ParentStudentLinks
            .Include(l => l.Student)
                .ThenInclude(s => s.User)
            .Where(l => l.ParentId == parentId)
            .OrderByDescending(l => l.LinkedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<ParentStudentLink?> GetByIdAsync(int id)
    {
        return _context.ParentStudentLinks
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public Task<bool> LinkExistsAsync(int parentId, int studentId)
    {
        return _context.ParentStudentLinks
            .AnyAsync(l => l.ParentId == parentId && l.StudentId == studentId);
    }

    public async Task AddAsync(ParentStudentLink link)
    {
        _context.ParentStudentLinks.Add(link);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ParentStudentLink link)
    {
        _context.ParentStudentLinks.Remove(link);
        await _context.SaveChangesAsync();
    }

    public async Task<StudentProfile?> GetLinkedStudentProfileAsync(int parentId, int studentId)
    {
        var isLinked = await _context.ParentStudentLinks
            .AnyAsync(l => l.ParentId == parentId && l.StudentId == studentId);

        if (!isLinked) return null;

        return await _context.StudentProfiles!
            .Include(s => s.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.StudentId == studentId);
    }

    public Task<int> CountCompletedLessonsAsync(int studentId)
    {
        return _context.Progresses!
            .Where(p => p.StudentId == studentId
                     && p.CompletionStatus == "Completed"
                     && p.IsBestAttempt)
            .CountAsync();
    }

    public Task<int> CountBadgesAsync(int studentId)
    {
        return _context.StudentBadges!
            .Where(sb => sb.StudentId == studentId)
            .CountAsync();
    }

    public async Task<double?> GetAverageQuizScoreAsync(int studentId)
    {
        var hasData = await _context.QuizAttempts!
            .AnyAsync(qa => qa.StudentId == studentId);

        if (!hasData) return null;

        return await _context.QuizAttempts!
            .Where(qa => qa.StudentId == studentId)
            .AverageAsync(qa => (double)qa.Score);
    }

    public Task<List<Progress>> GetRecentProgressAsync(int studentId, int take)
    {
        return _context.Progresses!
            .Include(p => p.Lesson)
            .Where(p => p.StudentId == studentId && p.IsBestAttempt)
            .OrderByDescending(p => p.CompletedAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<List<WeeklyAssignment>> GetUpcomingAssignmentsAsync(int take)
    {
        return _context.WeeklyAssignments!
            .Include(wa => wa.Lesson)
            .Where(wa => wa.IsVisible && wa.DueDate >= DateTime.Today)
            .OrderBy(wa => wa.DueDate)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<List<StudentBadge>> GetRecentBadgesAsync(int studentId, int take)
    {
        return _context.StudentBadges!
            .Include(sb => sb.Badge)
            .Where(sb => sb.StudentId == studentId)
            .OrderByDescending(sb => sb.EarnedAt)
            .Take(take)
            .AsNoTracking()
            .ToListAsync();
    }
}
