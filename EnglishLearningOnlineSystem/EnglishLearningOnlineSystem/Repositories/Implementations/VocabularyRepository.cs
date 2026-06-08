using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

// Repository xử lý dữ liệu từ vựng trong bài học
public class VocabularyRepository : IVocabularyRepository
{
    private readonly AppDbContext _db;

    public VocabularyRepository(AppDbContext db)
    {
        _db = db;
    }

    // Lấy bài học kèm danh sách từ vựng
    public async Task<Lesson?> GetLessonWithVocabAsync(int lessonId)
    {
        return await _db.Lessons!
            .Include(l => l.Course)
            .Include(l => l.Vocabularies)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.IsPublished);
    }

    // Lấy tất cả từ vựng từ các bài học đã published, kèm thông tin lesson/course
    public async Task<List<Vocabulary>> GetAllVocabByStudentAsync(int studentId)
    {
        // Lấy các lessonId được giao cho học sinh qua WeeklyAssignment
        var assignedLessonIds = await _db.WeeklyAssignments!
            .Where(wa => wa.IsVisible && wa.LessonId != null)
            .Select(wa => wa.LessonId!.Value)
            .Distinct()
            .ToListAsync();

        return await _db.Vocabularies!
            .Include(v => v.Lesson)
                .ThenInclude(l => l.Course)
            .Where(v => assignedLessonIds.Contains(v.LessonId)
                     && v.Lesson.IsPublished)
            .OrderBy(v => v.Lesson.Course.CourseName)
            .ThenBy(v => v.Lesson.OrderIndex)
            .ThenBy(v => v.Word)
            .ToListAsync();
    }
}
