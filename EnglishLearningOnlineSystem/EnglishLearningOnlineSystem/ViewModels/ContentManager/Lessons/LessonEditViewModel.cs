using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Lessons;

public class LessonEditViewModel
{
    public int LessonId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn khoá học")]
    [Display(Name = "Khoá học")]
    public int CourseId { get; set; }

    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [Display(Name = "Tiêu đề bài học")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Chủ đề là bắt buộc")]
    [Display(Name = "Chủ đề (Topic)")]
    public string Topic { get; set; } = string.Empty;

    [Range(0, 1000, ErrorMessage = "XP thưởng phải từ 0 đến 1000")]
    [Display(Name = "XP thưởng")]
    public int XPReward { get; set; } = 50;

    [Range(1, 120, ErrorMessage = "Thời gian học dự kiến phải từ 1 đến 120 phút")]
    [Display(Name = "Thời gian dự kiến (phút)")]
    public int EstimatedMinutes { get; set; } = 15;

    [Display(Name = "Thứ tự hiển thị")]
    public int OrderIndex { get; set; } = 1;

    [Display(Name = "Trạng thái hiển thị")]
    public bool IsPublished { get; set; } = true;
}
