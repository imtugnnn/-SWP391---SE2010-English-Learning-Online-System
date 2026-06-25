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

    // Content Manager
    public async Task<(List<EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyListItemViewModel> Items, int TotalCount)> GetVocabulariesAsync(string? keyword, int? lessonId, int page, int pageSize)
    {
        var (vocabularies, totalCount) = await _repo.GetVocabulariesPaginatedAsync(keyword, lessonId, page, pageSize);
        var items = vocabularies.Select(v => new EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyListItemViewModel
        {
            VocabularyId = v.VocabularyId,
            Word = v.Word,
            Meaning = v.Meaning,
            LessonTitle = v.Lesson?.Title ?? "",
            CourseName = v.Lesson?.Course?.CourseName ?? ""
        }).ToList();

        return (items, totalCount);
    }

    public async Task<(EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyEditViewModel? Model, string? ErrorMessage)> GetVocabularyForEditAsync(int id)
    {
        var v = await _repo.GetVocabularyByIdAsync(id);
        if (v == null) return (null, "Không tìm thấy từ vựng.");

        var model = new EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyEditViewModel
        {
            VocabularyId = v.VocabularyId,
            Word = v.Word,
            Meaning = v.Meaning,
            ImageUrl = v.ImageUrl,
            ExampleSentence = v.ExampleSentence,
            AudioUrl = v.AudioUrl,
            LessonId = v.LessonId
        };
        return (model, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateVocabularyAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyCreateViewModel model)
    {
        var vocabulary = new EnglishLearningOnlineSystem.Models.Vocabulary
        {
            Word = model.Word,
            Meaning = model.Meaning,
            ImageUrl = model.ImageUrl ?? "",
            ExampleSentence = model.ExampleSentence,
            AudioUrl = model.AudioUrl,
            LessonId = model.LessonId
        };

        await _repo.AddVocabularyAsync(vocabulary);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateVocabularyAsync(EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies.VocabularyEditViewModel model)
    {
        var vocabulary = await _repo.GetVocabularyByIdAsync(model.VocabularyId);
        if (vocabulary == null) return (false, "Không tìm thấy từ vựng.");

        vocabulary.Word = model.Word;
        vocabulary.Meaning = model.Meaning;
        vocabulary.ImageUrl = model.ImageUrl ?? "";
        vocabulary.ExampleSentence = model.ExampleSentence;
        vocabulary.AudioUrl = model.AudioUrl;
        vocabulary.LessonId = model.LessonId;

        await _repo.UpdateVocabularyAsync(vocabulary);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteVocabularyAsync(int id)
    {
        var vocabulary = await _repo.GetVocabularyByIdAsync(id);
        if (vocabulary == null) return (false, "Không tìm thấy từ vựng.");

        await _repo.DeleteVocabularyAsync(vocabulary);
        return (true, null);
    }
}