using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Student.Games;

// ── Hiển thị màn chơi Ghép từ (Matching) ─────────────────────────────────────

public class MatchingPlayViewModel
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;

    // Cột bên trái: các từ (đã được xáo trộn vị trí)
    public List<MatchingItem> Words { get; set; } = [];

    // Cột bên phải: các nghĩa tương ứng (xáo trộn vị trí khác với Words)
    public List<MatchingItem> Meanings { get; set; } = [];
}

public class MatchingItem
{
    public int VocabularyId { get; set; }
    public string Text { get; set; } = string.Empty;
}

// ── Submit đáp án ─────────────────────────────────────────────────────────────

public class MatchingSubmitViewModel
{
    [Required]
    public int GameId { get; set; }

    public List<MatchingAnswerItem> Answers { get; set; } = [];
}

public class MatchingAnswerItem
{
    [Required]
    public int VocabularyId { get; set; }

    // Id của vocabulary mà học sinh chọn làm nghĩa tương ứng (0 nếu chưa chọn)
    public int SelectedMeaningVocabularyId { get; set; }
}

// ── Kết quả ───────────────────────────────────────────────────────────────────

public class MatchingResultViewModel
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int LessonId { get; set; }

    public List<MatchingResultItem> Items { get; set; } = [];

    public int CorrectCount { get; set; }
    public int TotalCount { get; set; }
    public int XPEarned { get; set; }
    public int NewTotalXP { get; set; }
}

public class MatchingResultItem
{
    public string Word { get; set; } = string.Empty;
    public string YourMeaning { get; set; } = string.Empty;
    public string CorrectMeaning { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
