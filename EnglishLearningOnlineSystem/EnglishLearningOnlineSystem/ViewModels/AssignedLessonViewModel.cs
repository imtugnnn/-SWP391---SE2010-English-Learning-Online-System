namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel hiển thị danh sách bài học được giao cho học sinh
public class AssignedLessonListViewModel
{
    public List<AssignedLessonItem> Lessons { get; set; } = new();

    // Trạng thái lọc hiện tại
    public string FilterStatus { get; set; } = "";
}

// Thông tin hiển thị của một bài học được giao
public class AssignedLessonItem
{
    public int LessonId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Topic { get; set; } = string.Empty;

    public string CourseName { get; set; } = string.Empty;

    public int XPReward { get; set; }

    public int EstimatedMinutes { get; set; }
    public int VocabularyCount { get; set; }
    public int QuizCount { get; set; }
    public int MiniGameCount { get; set; }

    public DateTime WeekStartDate { get; set; }

    public DateTime DueDate { get; set; }

    // Trạng thái hoàn thành của bài học
    public string CompletionStatus { get; set; } = "NOT_STARTED";

    // Điểm quiz đạt được
    public int QuizScore { get; set; }

    // Kiểm tra bài học đã quá hạn hay chưa
    public bool IsOverdue => DateTime.Today > DueDate && CompletionStatus != "Completed";
}
