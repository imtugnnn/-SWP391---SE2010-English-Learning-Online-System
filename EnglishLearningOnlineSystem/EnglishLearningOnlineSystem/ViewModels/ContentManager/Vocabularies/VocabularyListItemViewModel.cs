namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Vocabularies
{
    public class VocabularyListItemViewModel
    {
        public int VocabularyId { get; set; }
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}
