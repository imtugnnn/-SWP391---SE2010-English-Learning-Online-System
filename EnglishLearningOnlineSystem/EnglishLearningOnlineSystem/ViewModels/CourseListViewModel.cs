namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel hiển thị danh sách khóa học cho học sinh (kèm filter)
public class CourseListViewModel
{
    public List<CourseSummary> Courses { get; set; } = new();

    // Bộ lọc tìm kiếm
    public string Keyword { get; set; } = string.Empty;
    public string Grade { get; set; } = string.Empty;

    // Danh sách grade dùng cho dropdown filter
    public List<string> Grades { get; set; } = new();
}

// Thông tin tóm tắt của một khóa học
public class CourseSummary
{
    public int CourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;

    public string GradeLevel { get; set; } = string.Empty;

    // Số lượng bài học trong khóa học
    public int LessonCount { get; set; }

    // Trạng thái học sinh đã đăng ký khóa học hay chưa
    public bool IsEnrolled { get; set; }
}