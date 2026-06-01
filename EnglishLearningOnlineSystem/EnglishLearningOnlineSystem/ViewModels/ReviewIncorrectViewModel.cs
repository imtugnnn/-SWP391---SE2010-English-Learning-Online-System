namespace EnglishLearningOnlineSystem.ViewModels;

public class ReviewIncorrectViewModel
{
    public int AttemptId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int Score { get; set; }
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool ShowAll { get; set; }
    public List<ReviewAnswerItem> Items { get; set; } = new();

    // Dashboard shared data
    public string Nickname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Level { get; set; }
}

public class ReviewAnswerItem
{
    public int QuizId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuizType { get; set; } = string.Empty;
    public string YourAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public List<string> AllOptions { get; set; } = new();
}
