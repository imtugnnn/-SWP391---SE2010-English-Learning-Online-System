namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherLessonPreviewViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
    public int OrderIndex { get; set; }
    public List<TeacherVocabularyPreviewViewModel> Vocabularies { get; set; } = new();
    public List<TeacherQuizPreviewViewModel> Quizzes { get; set; } = new();
    public List<TeacherMiniGamePreviewViewModel> MiniGames { get; set; } = new();
}

public class TeacherVocabularyPreviewViewModel
{
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
}

public class TeacherQuizPreviewViewModel
{
    public string Question { get; set; } = string.Empty;
    public string QuizType { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public string CorrectAnswer { get; set; } = string.Empty;
}

public class TeacherMiniGamePreviewViewModel
{
    public string Title { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
    public int XPReward { get; set; }
}
