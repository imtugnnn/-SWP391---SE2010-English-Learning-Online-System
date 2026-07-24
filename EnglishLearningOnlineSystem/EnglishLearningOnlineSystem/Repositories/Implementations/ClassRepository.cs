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
    public async Task<StudentProfile?> GetStudentProfileByIdAsync(int studentId)
    {
        return await _context.StudentProfiles!
            .Include(sp => sp.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.StudentId == studentId);
    }

    public async Task<List<Progress>> GetStudentProgressByStudentIdAsync(int studentId)
    {
        return await _context.Progresses!
            .Include(p => p.Lesson)
            .AsNoTracking()
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CompletedAt)
            .ToListAsync();
    }

    public async Task<List<TeacherFeedback>> GetTeacherFeedbackByStudentIdAsync(int studentId)
    {
        return await _context.TeacherFeedbacks!
            .Include(f => f.Teacher)
            .AsNoTracking()
            .Where(f => f.StudentId == studentId)
            .OrderByDescending(f => f.CreateAt)
            .ToListAsync();
    }
    public async Task AddTeacherFeedbackAsync(TeacherFeedback feedback)
    {
        _context.TeacherFeedbacks!.Add(feedback);
        await _context.SaveChangesAsync();
    }
    public async Task<List<Class>> GetClassesByTeacherIdAsync(int teacherId)
    {
        return await _context.Classes!
            .Include(c => c.Teacher)
            .Include(c => c.AcademicYear)
            .Where(c => c.TeacherId == teacherId && !c.IsDeleted)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task UpdateClassCourseAsync(int classId, int courseId)
    {
        var classEntity = await _context.Classes!
            .FirstOrDefaultAsync(c => c.ClassId == classId);

        if (classEntity == null)
        {
            return;
        }

        classEntity.CourseId = courseId;
    }

    public async Task AddNotificationsAsync(List<Notification> notifications)
    {
        await _context.Notifications!.AddRangeAsync(notifications);
    }

    public async Task AddNotificationAsync(Notification notification)
    {
        await _context.Notifications!.AddAsync(notification);
        await _context.SaveChangesAsync();
    }
}
