using EnglishLearningOnlineSystem.ViewModels.Student.Games;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IWordScrambleService
{
    /// <summary>Lấy danh sách game Word Scramble theo bài học.</summary>
    Task<StudentMiniGameListViewModel?> GetGamesByLessonAsync(int lessonId);

    /// <summary>Tải màn chơi: chọn ngẫu nhiên từ vựng và xáo trộn chữ cái.</summary>
    Task<WordScramblePlayViewModel?> LoadPlayAsync(int gameId, int studentId, int? assignmentId = null);

    /// <summary>
    /// Kiểm tra đáp án, lưu StudentGameProgress, cập nhật XP và trả về kết quả.
    /// </summary>
    Task<(WordScrambleResultViewModel? Result, string? Error)> SubmitAsync(
        WordScrambleSubmitViewModel vm,
        int studentId);
}
