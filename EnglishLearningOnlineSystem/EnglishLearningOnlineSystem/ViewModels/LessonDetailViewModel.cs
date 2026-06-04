namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel hiển thị chi tiết bài học và tiến độ học tập của học sinh
public class LessonDetailViewModel
{
    // Thông tin bài học
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int EstimatedMinutes { get; set; }

    // Nội dung bài học
    public List<VocabItem> Vocabularies { get; set; } = new();
    public List<QuizItem> Quizzes { get; set; } = new();
    public List<MiniGameItem> MiniGames { get; set; } = new();

    // Tiến độ học tập
    public string CompletionStatus { get; set; } = "NOT_STARTED";
    public int BestScore { get; set; }
    public int AttemptCount { get; set; }

    // Kiểm tra bài học đã hoàn thành hay chưa
    public bool IsCompleted => CompletionStatus == "Completed";
}

// Thông tin từ vựng trong bài học
public class VocabItem
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
}

// Thông tin câu hỏi quiz
public class QuizItem
{
    public int QuizId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuizType { get; set; } = string.Empty;
    public string OptionsJson { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
}

// Thông tin mini game trong bài học
public class MiniGameItem
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public int XPReward { get; set; }

    // Trạng thái hoàn thành mini game
    public bool IsDone { get; set; }
}