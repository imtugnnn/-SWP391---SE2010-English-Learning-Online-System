using EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;

// ── LessonDetailsViewModel ────────────────────────────────────────────────────

public class LessonDetailsViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
    public bool IsPublished { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string CourseGradeLevel { get; set; } = string.Empty;
    public List<MiniGameListItemViewModel> MiniGames { get; set; } = [];
}

// ── LessonListItemViewModel ───────────────────────────────────────────────────

public class LessonListItemViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
    public bool IsPublished { get; set; }
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
}

// ── LessonCreateViewModel ─────────────────────────────────────────────────────

public class LessonCreateViewModel
{
    [Required(ErrorMessage = "Vui lòng chọn khoá học.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khoá học.")]
    [Display(Name = "Khoá học")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không được quá 255 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Chủ đề không được quá 255 ký tự.")]
    [Display(Name = "Chủ đề")]
    public string? Topic { get; set; }

    [Range(0, 9999)]
    [Display(Name = "Thứ tự")]
    public int OrderIndex { get; set; } = 1;

    [Range(1, 999, ErrorMessage = "Thời lượng phải từ 1 đến 999 phút.")]
    [Display(Name = "Thời lượng (phút)")]
    public int EstimatedMinutes { get; set; } = 30;

    [Range(0, 9999)]
    [Display(Name = "XP Thưởng")]
    public int XPReward { get; set; } = 0;

    [Display(Name = "Hiển thị")]
    public bool IsPublished { get; set; } = false;
}

// ── LessonEditViewModel ───────────────────────────────────────────────────────

public class LessonEditViewModel
{
    public int LessonId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khoá học.")]
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn khoá học.")]
    [Display(Name = "Khoá học")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
    [MaxLength(255, ErrorMessage = "Tiêu đề không được quá 255 ký tự.")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Chủ đề không được quá 255 ký tự.")]
    [Display(Name = "Chủ đề")]
    public string? Topic { get; set; }

    [Range(0, 9999)]
    [Display(Name = "Thứ tự")]
    public int OrderIndex { get; set; }

    [Range(1, 999, ErrorMessage = "Thời lượng phải từ 1 đến 999 phút.")]
    [Display(Name = "Thời lượng (phút)")]
    public int EstimatedMinutes { get; set; }

    [Range(0, 9999)]
    [Display(Name = "XP Thưởng")]
    public int XPReward { get; set; }

    [Display(Name = "Hiển thị")]
    public bool IsPublished { get; set; }
}
