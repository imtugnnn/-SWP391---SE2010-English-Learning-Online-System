using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

// Service xử lý nghiệp vụ học từ vựng trong bài học
public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _repo;

    public VocabularyService(IVocabularyRepository repo)
    {
        _repo = repo;
    }

    // Lấy danh sách từ vựng của một bài học để hiển thị
    public async Task<VocabularyViewModel?> GetVocabularyAsync(int lessonId)
    {
        var lesson = await _repo.GetLessonWithVocabAsync(lessonId);
        if (lesson == null) return null;

        return new VocabularyViewModel
        {
            LessonId = lesson.LessonId,
            LessonTitle = lesson.Title,
            CourseName = lesson.Course?.CourseName ?? "",

            Words = lesson.Vocabularies?.Select(v => new VocabLearningItem
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Meaning = v.Meaning,
                ImageUrl = v.ImageUrl ?? "",

                // Audio chưa được hỗ trợ trong hệ thống
                AudioUrl = ""
            }).ToList() ?? new()
        };
    }
}