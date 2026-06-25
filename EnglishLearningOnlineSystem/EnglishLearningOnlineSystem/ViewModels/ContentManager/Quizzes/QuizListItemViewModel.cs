namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Quizzes
{
    public class QuizListItemViewModel
    {
        public int QuizId { get; set; }
        public string Question { get; set; } = string.Empty;
        public string QuizType { get; set; } = string.Empty;
        public string LessonTitle { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
    }
}
