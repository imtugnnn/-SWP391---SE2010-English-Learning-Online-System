using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;

// ── Shared select-list item ───────────────────────────────────────────────────

public class LessonSelectItem
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
}

// ── Danh sách loại game hỗ trợ (mới thêm) ────────────────────────────────────

public static class GameTypeOptions
{
    public static readonly (string Value, string Label)[] All =
    {
        ("WordScramble", "Xáo chữ (Word Scramble)"),
        ("Matching", "Ghép từ (Matching)")
    };

    public static string GetLabel(string value)
    {
        foreach (var opt in All)
        {
            if (opt.Value == value) return opt.Label;
        }
        return value;
    }
}

// ── MiniGameViewModel (đọc / chi tiết) ───────────────────────────────────────

public class MiniGameViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
}

// ── MiniGameDetailsViewModel ──────────────────────────────────────────────────

public class MiniGameDetailsViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
}

// ── MiniGameListItemViewModel ─────────────────────────────────────────────────

public class MiniGameListItemViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
}

// ── MiniGameListViewModel (trang danh sách phân trang) ───────────────────────

public class MiniGameListViewModel
{
    public IEnumerable<MiniGameListItemViewModel> Items { get; set; } = [];
    public List<LessonSelectItem> Lessons { get; set; } = [];

    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public string? SearchTitle { get; set; }
    public int? FilterLessonId { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// ── CreateMiniGameViewModel ───────────────────────────────────────────────────

public class CreateMiniGameViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn bài học.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn bài học hợp lệ.")]
    public int LessonId { get; set; }

    // Chỉ dùng để hiển thị
    public string LessonTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    // Trước đây cố định "WordScramble" — nay có thể chọn giữa các loại trong GameTypeOptions.All
    [Required(ErrorMessage = "Vui lòng chọn loại game.")]
    [Display(Name = "Loại game")]
    public string GameType { get; set; } = "WordScramble";

    [Range(0, 9999, ErrorMessage = "XP Thưởng phải từ 0 đến 9999.")]
    [Display(Name = "XP Thưởng")]
    public int XPReward { get; set; } = 10;
}

// ── EditMiniGameViewModel ─────────────────────────────────────────────────────

public class EditMiniGameViewModel
{
    public int GameId { get; set; }

    // LessonId bị khóa sau khi tạo — chỉ hiển thị
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không được vượt quá 255 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    // Loại game bị khóa sau khi tạo — chỉ đọc
    public string GameType { get; set; } = "WordScramble";

    [Range(0, 9999, ErrorMessage = "XP Thưởng phải từ 0 đến 9999.")]
    [Display(Name = "XP Thưởng")]
    public int XPReward { get; set; }
}
