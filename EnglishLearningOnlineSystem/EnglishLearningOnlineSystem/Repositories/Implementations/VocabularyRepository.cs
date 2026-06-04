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
}