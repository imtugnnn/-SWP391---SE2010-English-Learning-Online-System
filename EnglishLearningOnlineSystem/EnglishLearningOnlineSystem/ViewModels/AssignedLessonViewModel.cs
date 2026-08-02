namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel hiển thị danh sách bài học được giao cho học sinh
public class AssignedLessonListViewModel
{
    public List<AssignedLessonItem> Lessons { get; set; } = new();
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int InProgressCount { get; set; }
    public int NotStartedCount { get; set; }
    public int OverdueCount { get; set; }

    // Trạng thái lọc hiện tại
    public string FilterStatus { get; set; } = "";
}

// Thông tin hiển thị của một bài học được giao
public class AssignedLessonItem
{
    public int AssignmentId { get; set; }

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

    public string AssignmentPeriod =>
        $"{WeekStartDate:dd/MM/yyyy} - {DueDate:dd/MM/yyyy}";

    // Trạng thái hoàn thành của bài học
    public string CompletionStatus { get; set; } = "NOT_STARTED";

    // Điểm quiz đạt được
    public int QuizScore { get; set; }

    // Kiểm tra bài học đã quá hạn hay chưa
    public bool IsOverdue => DateTime.Today > DueDate && CompletionStatus != "Completed";
}
