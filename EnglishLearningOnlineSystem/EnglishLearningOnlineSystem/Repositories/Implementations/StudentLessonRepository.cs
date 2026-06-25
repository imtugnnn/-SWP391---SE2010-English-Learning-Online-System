using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

// Repository xử lý dữ liệu liên quan đến bài học của học sinh
public class StudentLessonRepository : IStudentLessonRepository
{
    private readonly AppDbContext _db;

    public StudentLessonRepository(AppDbContext db)
    {
        _db = db;
    }

    // Lấy danh sách bài học được giao cho học sinh
    public async Task<List<WeeklyAssignment>> GetAssignedLessonsAsync(int studentId)
    {
        return await _db.WeeklyAssignments!
            .Include(wa => wa.Lesson)
                .ThenInclude(l => l.Course)
            .Where(wa => wa.IsVisible && wa.LessonId != null)
            .OrderBy(wa => wa.DueDate)
            .ToListAsync();
    }

    // Lấy lần làm bài tốt nhất của học sinh cho một bài học
    public async Task<Progress?> GetBestProgressAsync(int studentId, int lessonId)
    {
        return await _db.Progresses!
            .Where(p => p.StudentId == studentId
                     && p.LessonId == lessonId
                     && p.IsBestAttempt)
            .FirstOrDefaultAsync();
    }
}