using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Student.Games;

// ── ViewModel để hiển thị màn chơi Word Scramble ─────────────────────────────

public class WordScramblePlayViewModel
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;

    // Từ vựng được chọn ngẫu nhiên
    public int VocabularyId { get; set; }
    public string ScrambledWord { get; set; } = string.Empty;   // Chữ đã xáo trộn
    public string Meaning { get; set; } = string.Empty;         // Gợi ý nghĩa
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
}

// ── ViewModel submit đáp án ───────────────────────────────────────────────────

public class WordScrambleSubmitViewModel
{
    [Required]
    public int GameId { get; set; }

    [Required]
    public int VocabularyId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập câu trả lời.")]
    [Display(Name = "Câu trả lời của bạn")]
    public string Answer { get; set; } = string.Empty;
}

// ── ViewModel hiển thị kết quả ────────────────────────────────────────────────

public class WordScrambleResultViewModel
{
    public int GameId { get; set; }
    public string GameTitle { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string CorrectWord { get; set; } = string.Empty;
    public string StudentAnswer { get; set; } = string.Empty;
    public int XPEarned { get; set; }
    public int NewTotalXP { get; set; }
    public int LessonId { get; set; }
}

// ── ViewModel danh sách game cho học sinh ────────────────────────────────────

public class StudentMiniGameListViewModel
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public IEnumerable<StudentMiniGameItemViewModel> Games { get; set; } = [];
}

public class StudentMiniGameItemViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int XPReward { get; set; }
}
