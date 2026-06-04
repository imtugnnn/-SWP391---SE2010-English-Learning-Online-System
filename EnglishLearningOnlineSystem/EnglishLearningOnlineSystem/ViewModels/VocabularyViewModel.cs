namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel hiển thị danh sách từ vựng của một bài học
public class VocabularyViewModel
{
    // Thông tin bài học
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;

    // Danh sách từ vựng
    public List<VocabLearningItem> Words { get; set; } = new();
}

// Thông tin một từ vựng trong bài học
public class VocabLearningItem
{
    public int VocabularyId { get; set; }

    public string Word { get; set; } = string.Empty;

    public string Meaning { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    // Đường dẫn file phát âm
    public string AudioUrl { get; set; } = string.Empty;
}