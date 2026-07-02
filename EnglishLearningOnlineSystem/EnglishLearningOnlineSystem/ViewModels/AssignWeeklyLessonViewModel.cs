using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class AssignWeeklyLessonViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chương trình học.")]
    public int? SelectedCourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;
    public bool HasCourse => CourseId.HasValue;

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    public DateTime WeekStartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng chọn hạn hoàn thành.")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

    public List<int> SelectedLessonIds { get; set; } = new();

    public List<CourseOptionViewModel> Courses { get; set; } = new();
    public List<AssignLessonItemViewModel> Lessons { get; set; } = new();
}

public class CourseOptionViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
}

public class AssignLessonItemViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
}