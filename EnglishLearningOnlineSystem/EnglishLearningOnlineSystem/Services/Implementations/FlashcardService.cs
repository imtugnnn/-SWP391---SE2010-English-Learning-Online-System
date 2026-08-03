using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

/// <summary>
/// Service phụ trách tính năng Luyện tập Flashcard (Từ vựng).
/// Xử lý logic tạo phiên học mới, trộn từ vựng ngẫu nhiên, lưu kết quả tự đánh giá
/// của học sinh ("Đã biết"/"Chưa biết") và tính toán kết quả cuối phiên.
/// </summary>
public class FlashcardService : IFlashcardService
{
    private readonly IFlashcardRepository _flashcardRepo;
    private readonly IStudentDashboardRepository _dashboardRepo;

    public FlashcardService(IFlashcardRepository flashcardRepo, IStudentDashboardRepository dashboardRepo)
    {
        _flashcardRepo = flashcardRepo;
        _dashboardRepo = dashboardRepo;
    }

    public async Task<FlashcardPracticeViewModel?> StartSessionAsync(
        int lessonId,
        int studentId,
        bool resetProgress = false,
        int? assignmentId = null)
    {
        var lesson = await _flashcardRepo.GetLessonByIdAsync(lessonId);
        if (lesson == null) return null;

        var vocabularies = await _flashcardRepo.GetVocabularyByLessonAsync(
            lessonId,
            studentId,
            assignmentId);
        if (!vocabularies.Any()) return null;

        if (resetProgress)
        {
            await _flashcardRepo.ResetMasteryAsync(studentId, lessonId);
        }

        // Adaptive Learning: Lấy danh sách các từ vựng học sinh đã đánh dấu là "Thuộc" trong các phiên trước
        var masteredIds = await _flashcardRepo.GetMasteredVocabularyIdsAsync(studentId, lessonId);
        
        // Lọc ra những từ chưa thuộc
        var vocabulariesToPractice = vocabularies.Where(v => !masteredIds.Contains(v.VocabularyId)).ToList();

        // Nếu học sinh đã thuộc hết (hoặc danh sách trống), trả về rỗng để hiển thị thông báo chúc mừng trên UI
        // Không tự động reset lại full list nữa
        if (!vocabulariesToPractice.Any())
        {
            vocabulariesToPractice = new List<Vocabulary>();
        }

        // Bắt đầu một phiên luyện tập (Session) mới cho học sinh
        var session = new FlashcardSession
        {
            StudentId = studentId,
            LessonId = lessonId,
            StartedAt = DateTime.UtcNow
        };

        await _flashcardRepo.CreateSessionAsync(session);
        var dashboard = await _dashboardRepo.GetProfileByUserIdAsync(studentId);

        return new FlashcardPracticeViewModel
        {
            SessionId = session.SessionId,
            LessonId = lessonId,
            LessonTitle = lesson.Title,
            Cards = vocabulariesToPractice.Select(v => new FlashcardItem
            {
                VocabularyId = v.VocabularyId,
                Word = v.Word,
                Meaning = v.Meaning,
                ExampleSentence = v.ExampleSentence,
                ImageUrl = v.ImageUrl,
                AudioUrl = v.AudioUrl
            }).ToList(),
            Nickname = dashboard?.Nickname ?? dashboard?.User?.Username ?? "Student",
            AvatarUrl = dashboard?.AvatarUrl ?? "/images/default-avatar.png",
            Level = dashboard?.Level ?? 1
        };
    }

    public async Task CompleteSessionAsync(int studentId, FlashcardCompleteViewModel completeData)
    {
        var session = await _flashcardRepo.GetSessionAsync(completeData.SessionId, studentId);
        if (session == null || session.CompletedAt.HasValue) return;

        // Lưu kết quả đánh giá từng thẻ flashcard ("KnewIt": đã thuộc hay chưa)
        var results = completeData.Results.Select(r => new FlashcardCardResult
        {
            SessionId = completeData.SessionId,
            VocabularyId = r.VocabularyId,
            KnewIt = r.KnewIt
        }).ToList();

        await _flashcardRepo.CompleteSessionAsync(completeData.SessionId, completeData.Results.Count, results);
    }

    public async Task<FlashcardResultViewModel?> GetSessionResultAsync(int sessionId, int studentId)
    {
        var session = await _flashcardRepo.GetSessionAsync(sessionId, studentId);
        if (session == null) return null;

        var dashboard = await _dashboardRepo.GetProfileByUserIdAsync(studentId);

        var items = session.CardResults.Select(r => new FlashcardSessionResultItem
        {
            Word = r.Vocabulary?.Word ?? "",
            Meaning = r.Vocabulary?.Meaning ?? "",
            KnewIt = r.KnewIt
        }).ToList();

        return new FlashcardResultViewModel
        {
            SessionId = session.SessionId,
            LessonId = session.LessonId,
            LessonTitle = session.Lesson?.Title ?? "",
            TotalCards = session.CardsReviewed,
            KnewCards = items.Count(i => i.KnewIt),
            Items = items,
            Nickname = dashboard?.Nickname ?? dashboard?.User?.Username ?? "Student",
            AvatarUrl = dashboard?.AvatarUrl ?? "/images/default-avatar.png",
            Level = dashboard?.Level ?? 1
        };
    }
}
