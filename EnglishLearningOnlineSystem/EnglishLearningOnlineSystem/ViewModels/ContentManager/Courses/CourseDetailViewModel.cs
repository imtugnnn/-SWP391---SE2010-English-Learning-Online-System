namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Courses
{
    public class CourseDetailViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string GradeLevel { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublished { get; set; }
        public bool IsDeleted { get; set; }
        public string? CreatorName { get; set; }
        public int LessonCount { get; set; }
        public int TotalDurationMinutes { get; set; }

        public List<CourseLessonItem> Lessons { get; set; } = new();

        public class CourseLessonItem
        {
            public int LessonId { get; set; }
            public string Title { get; set; } = string.Empty;
            public string Topic { get; set; } = string.Empty;
            public int EstimatedMinutes { get; set; }
            public bool IsPublished { get; set; }
            public int OrderIndex { get; set; }
        }
    }
}