namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel trang từ vựng tổng hợp — hiển thị tất cả từ vựng
public class VocabularyHubViewModel
{
    public List<VocabHubItem> Words { get; set; } = new();
}

// Thông tin một từ vựng trong trang hub
public class VocabHubItem
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
}