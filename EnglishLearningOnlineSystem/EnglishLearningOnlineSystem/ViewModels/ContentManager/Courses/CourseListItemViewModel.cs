namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Courses
{
    public class CourseListItemViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public int LessonCount { get; set; }
    }
}