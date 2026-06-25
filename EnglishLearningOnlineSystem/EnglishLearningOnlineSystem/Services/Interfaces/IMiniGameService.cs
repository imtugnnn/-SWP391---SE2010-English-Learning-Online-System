using EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IMiniGameService
{
    Task<MiniGameListViewModel> GetPagedAsync(
        int? lessonId,
        string? searchTitle,
        int page,
        int pageSize);

    Task<MiniGameViewModel?> GetByIdAsync(int gameId);
    Task<MiniGameDetailsViewModel?> GetDetailsAsync(int gameId);
    Task<CreateMiniGameViewModel?> BuildCreateViewModelAsync(int lessonId);
    Task<EditMiniGameViewModel?> BuildEditViewModelAsync(int gameId);

    /// <summary>Lấy LessonId của game (dùng trước khi xóa để redirect đúng bài học).</summary>
    Task<int?> GetLessonIdAsync(int gameId);

    /// <returns>null nếu thành công, chuỗi lỗi nếu thất bại.</returns>
    Task<string?> CreateAsync(CreateMiniGameViewModel vm);

    /// <returns>null nếu thành công, chuỗi lỗi nếu thất bại.</returns>
    Task<string?> UpdateAsync(EditMiniGameViewModel vm);

    /// <returns>null nếu thành công, chuỗi lỗi nếu thất bại.</returns>
    Task<string?> DeleteAsync(int gameId);
}