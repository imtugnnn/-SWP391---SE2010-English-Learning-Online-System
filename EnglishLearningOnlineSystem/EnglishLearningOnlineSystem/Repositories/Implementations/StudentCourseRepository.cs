using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

// Repository xử lý dữ liệu liên quan đến khóa học của học sinh
public class StudentCourseRepository : IStudentCourseRepository
{
    private readonly AppDbContext _db;

    public StudentCourseRepository(AppDbContext db)
    {
        _db = db;
    }

    // Lấy danh sách khóa học đã publish theo keyword và grade
    public async Task<List<Course>> GetAllPublishedAsync(string keyword, string grade)
    {
        var query = _db.Courses!.Where(c => c.IsPublished);

        if (!string.IsNullOrWhiteSpace(keyword))
            query = query.Where(c => c.CourseName.Contains(keyword));

        if (!string.IsNullOrWhiteSpace(grade))
            query = query.Where(c => c.GradeLevel == grade);

        return await query
            .OrderBy(c => c.GradeLevel)
            .ThenBy(c => c.CourseName)
            .ToListAsync();
    }

    // Lấy danh sách các khối lớp có khóa học published
    public async Task<List<string>> GetAllGradesAsync()
    {
        return await _db.Courses!
            .Where(c => c.IsPublished && c.GradeLevel != null)
            .Select(c => c.GradeLevel)
            .Distinct()
            .OrderBy(g => g)
            .ToListAsync();
    }

    // Lấy danh sách courseId mà học sinh đã tham gia
    public async Task<List<int>> GetEnrolledCourseIdsAsync(int studentId)
    {
        var profile = await _db.StudentProfiles!
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.StudentId == studentId);

        if (profile == null)
            return new List<int>();

        return await _db.Classes!
            .Where(c => c.CourseId.HasValue)
            .Select(c => c.CourseId!.Value)
            .Distinct()
            .ToListAsync();
    }

    // Đếm số bài học trong một khóa học
    public async Task<int> GetLessonCountAsync(int courseId)
    {
        return await _db.Lessons!
            .Where(l => l.CourseId == courseId && l.IsPublished)
            .CountAsync();
    }

    // Lấy thông tin course kèm danh sách lessons đã publish
    public async Task<Course?> GetCourseWithLessonsAsync(int courseId)
    {
        return await _db.Courses!
            .Include(c => c.Lessons.Where(l => l.IsPublished)
                .OrderBy(l => l.OrderIndex))
            .FirstOrDefaultAsync(c => c.CourseId == courseId && c.IsPublished);
    }
}
