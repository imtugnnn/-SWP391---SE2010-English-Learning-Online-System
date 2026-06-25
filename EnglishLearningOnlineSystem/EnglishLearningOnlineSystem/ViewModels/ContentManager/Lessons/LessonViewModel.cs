using EnglishLearningOnlineSystem.ViewModels.ContentManager.Minigames;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;

// ── Shared select-list item ───────────────────────────────────────────────────

public class CourseSelectItem
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
}

// ── LessonViewModel (read / detail) ──────────────────────────────────────────

public class LessonViewModel
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

    // Danh sách mini game thuộc bài học này (hiển thị trong tab/section bên dưới)
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

// ── LessonListViewModel (paged list page) ────────────────────────────────────

public class LessonListViewModel
{
    public IEnumerable<LessonListItemViewModel> Items { get; set; } = [];
    public List<CourseSelectItem> Courses { get; set; } = [];

    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public string? SearchTitle { get; set; }
    public int? FilterCourseId { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

// ── CreateLessonViewModel ─────────────────────────────────────────────────────

public class CreateLessonViewModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int CourseId { get; set; }

    // display only
    public string CourseName { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Topic { get; set; }

    [Range(0, 9999)]
    public int OrderIndex { get; set; } = 1;

    [Range(1, 999)]
    public int EstimatedMinutes { get; set; } = 30;

    [Range(0, 9999)]
    public int XPReward { get; set; } = 0;

    public bool IsPublished { get; set; } = false;
}

// ── EditLessonViewModel ───────────────────────────────────────────────────────

public class EditLessonViewModel
{
    public int LessonId { get; set; }

    // Read-only display — CourseId is locked after creation
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters.")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Topic cannot exceed 255 characters.")]
    public string? Topic { get; set; }

    [Required(ErrorMessage = "Order index is required.")]
    [Range(0, 9999, ErrorMessage = "Order index must be between 0 and 9999.")]
    [Display(Name = "Order Index")]
    public int OrderIndex { get; set; }

    [Required(ErrorMessage = "Estimated duration is required.")]
    [Range(1, 999, ErrorMessage = "Estimated duration must be between 1 and 999 minutes.")]
    [Display(Name = "Estimated Duration (minutes)")]
    public int EstimatedMinutes { get; set; }

    [Range(0, 9999, ErrorMessage = "XP Reward must be between 0 and 9999.")]
    [Display(Name = "XP Reward")]
    public int XPReward { get; set; }

    [Display(Name = "Published")]
    public bool IsPublished { get; set; }

    // Not used in form submission — kept for display convenience
    public List<CourseSelectItem> Courses { get; set; } = [];
}