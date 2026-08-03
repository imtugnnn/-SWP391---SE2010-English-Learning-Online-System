namespace EnglishLearningOnlineSystem.ViewModels;

public class FlashcardResultViewModel
{
    public int SessionId { get; set; }
    public int LessonId { get; set; }
    public int? AssignmentId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int TotalCards { get; set; }
    public int KnewCards { get; set; }
    public List<FlashcardSessionResultItem> Items { get; set; } = new();

    public string Nickname { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public int Level { get; set; }
}

public class FlashcardSessionResultItem
{
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
    public bool KnewIt { get; set; }
}
