using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class ClassRepository : IClassRepository
{
    private readonly AppDbContext _context;

    public ClassRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Class?> GetClassDetailByIdAsync(int classId)
    {
        return await _context.Classes!
            .Include(c => c.Teacher)
            .Include(c => c.AcademicYear)
            .Include(c => c.Course)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ClassId == classId);
    }

    public async Task<List<ClassEnrollment>> GetActiveStudentsByClassIdAsync(int classId)
    {
        return await _context.ClassEnrollments!
            .Include(e => e.Student)
            .Where(e => e.ClassId == classId && e.Student.IsActive)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<WeeklyAssignment>> GetAssignmentsByClassCourseAsync(int? courseId)
    {
        if (courseId == null)
        {
            return new List<WeeklyAssignment>();
        }

        return await _context.WeeklyAssignments!
            .Include(a => a.Lesson)
            .Where(a => a.CourseId == courseId && a.IsVisible)
            .AsNoTracking()
            .OrderByDescending(a => a.WeekStartDate)
            .ToListAsync();
    }

    public async Task<List<Progress>> GetProgressByStudentIdsAndLessonIdsAsync(List<int> studentIds, List<int> lessonIds)
    {
        if (!studentIds.Any() || !lessonIds.Any())
        {
            return new List<Progress>();
        }

        return await _context.Progresses!
            .Where(p => studentIds.Contains(p.StudentId) && lessonIds.Contains(p.LessonId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ClassEnrollment>> GetStudentsByClassIdAsync(int classId)
    {
        return await _context.ClassEnrollments!
            .Include(e => e.Student)
            .Where(e => e.ClassId == classId)
            .AsNoTracking()
            .ToListAsync();
    }
}