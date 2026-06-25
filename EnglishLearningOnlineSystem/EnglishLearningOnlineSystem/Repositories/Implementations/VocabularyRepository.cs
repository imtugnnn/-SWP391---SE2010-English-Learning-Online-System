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

    // Content Manager CRUD
    public async Task<(List<Vocabulary> Items, int TotalCount)> GetVocabulariesPaginatedAsync(string? keyword, int? lessonId, int page, int pageSize)
    {
        var query = _db.Vocabularies!
            .Include(v => v.Lesson)
            .ThenInclude(l => l.Course)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var lowerKeyword = keyword.ToLower();
            query = query.Where(v => v.Word.ToLower().Contains(lowerKeyword) || v.Meaning.ToLower().Contains(lowerKeyword));
        }

        if (lessonId.HasValue && lessonId.Value > 0)
        {
            query = query.Where(v => v.LessonId == lessonId.Value);
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.VocabularyId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Vocabulary?> GetVocabularyByIdAsync(int id)
    {
        return await _db.Vocabularies!
            .Include(v => v.Lesson)
            .FirstOrDefaultAsync(v => v.VocabularyId == id);
    }

    public async Task AddVocabularyAsync(Vocabulary vocabulary)
    {
        _db.Vocabularies!.Add(vocabulary);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateVocabularyAsync(Vocabulary vocabulary)
    {
        _db.Vocabularies!.Update(vocabulary);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteVocabularyAsync(Vocabulary vocabulary)
    {
        _db.Vocabularies!.Remove(vocabulary);
        await _db.SaveChangesAsync();
    }
}
