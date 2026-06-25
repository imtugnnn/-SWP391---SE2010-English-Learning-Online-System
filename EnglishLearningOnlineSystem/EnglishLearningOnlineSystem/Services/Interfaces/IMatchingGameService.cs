using EnglishLearningOnlineSystem.ViewModels.Student.Games;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IMatchingGameService
{
    /// <summary>Tải màn chơi: chọn ngẫu nhiên các cặp từ vựng và xáo trộn cột nghĩa.</summary>
    Task<MatchingPlayViewModel?> LoadPlayAsync(int gameId);

    /// <summary>
    /// Kiểm tra các cặp ghép, lưu StudentGameProgress, cập nhật XP và trả về kết quả.
    /// </summary>
    Task<(MatchingResultViewModel? Result, string? Error)> SubmitAsync(
        MatchingSubmitViewModel vm,
        int studentId);
}
