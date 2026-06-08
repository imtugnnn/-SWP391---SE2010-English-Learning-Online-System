using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class VocabularyService : IVocabularyService
{
    private readonly IVocabularyRepository _repo;

    public VocabularyService(IVocabularyRepository repo)
    {
        _repo = repo;
    }

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
                AudioUrl = v.AudioUrl ?? ""
            }).ToList() ?? new()
        };
    }

    // Lấy toàn bộ từ vựng từ các bài học được giao, trả về flat list
    public async Task<VocabularyHubViewModel> GetAllVocabularyAsync(int studentId)
    {
        var allVocab = await _repo.GetAllVocabByStudentAsync(studentId);

        return new VocabularyHubViewModel
        {
            Words = allVocab.Select(v => new VocabHubItem
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Meaning = v.Meaning,
                ImageUrl = v.ImageUrl ?? "",
                LessonTitle = v.Lesson?.Title ?? "",
                CourseName = v.Lesson?.Course?.CourseName ?? ""
            }).ToList()
        };
    }
}