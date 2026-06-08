namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel hiển thị chi tiết khóa học và danh sách bài học
public class CourseDetailViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
    public bool IsEnrolled { get; set; }

    public List<CourseLessonItem> Lessons { get; set; } = new();
}

// Thông tin tóm tắt bài học trong một khóa học
public class CourseLessonItem
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int EstimatedMinutes { get; set; }
    public int OrderIndex { get; set; }

    // Tiến độ học sinh
    public string CompletionStatus { get; set; } = "NOT_STARTED";
    public int BestScore { get; set; }
}
