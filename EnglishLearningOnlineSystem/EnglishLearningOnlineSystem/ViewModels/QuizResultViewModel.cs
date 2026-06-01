namespace EnglishLearningOnlineSystem.ViewModels;

public class QuizResultViewModel
{
    public int AttemptId { get; set; }
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int Score { get; set; }           // Percentage 0-100
    public int CorrectCount { get; set; }
    public int TotalQuestions { get; set; }
    public int XpEarned { get; set; }
    public bool AlreadyAwardedXp { get; set; }
    public int TimeSpentSec { get; set; }
    public List<QuizAnswerResultItem> AnswerResults { get; set; } = new();
}

public class QuizAnswerResultItem
{
    public int QuizId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuizType { get; set; } = string.Empty;
    public string SelectedAnswer { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public List<string> AllOptions { get; set; } = new();
}
