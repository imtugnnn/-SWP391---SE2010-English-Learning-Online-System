namespace EnglishLearningOnlineSystem.ViewModels;

public class FlashcardPracticeViewModel
{
    public int SessionId { get; set; }
    public int LessonId { get; set; }
    public int? AssignmentId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public List<FlashcardItem> Cards { get; set; } = new();

    public string Nickname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Level { get; set; }
}

public class FlashcardItem
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public string? ExampleSentence { get; set; }
    public string? ImageUrl { get; set; }
    public string? AudioUrl { get; set; }
}

public class FlashcardCompleteViewModel
{
    public int SessionId { get; set; }
    public int LessonId { get; set; }
    public int? AssignmentId { get; set; }
    public List<FlashcardResultItem> Results { get; set; } = new();
}

public class FlashcardResultItem
{
    public int VocabularyId { get; set; }
    public bool KnewIt { get; set; }
}
