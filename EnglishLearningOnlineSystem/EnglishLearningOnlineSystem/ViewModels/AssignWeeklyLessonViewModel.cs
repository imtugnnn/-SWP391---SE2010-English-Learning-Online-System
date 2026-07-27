using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class AssignWeeklyLessonViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn chương trình học.")]
    public int? SelectedCourseId { get; set; }

    public string CourseName { get; set; } = string.Empty;
    public bool HasCourse => CourseId.HasValue;
    public int ActiveStudentCount { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    public DateTime WeekStartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Vui lòng chọn hạn hoàn thành.")]
    public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

    [Required(ErrorMessage = "Vui lòng chọn trạng thái bài giao.")]
    public EnglishLearningOnlineSystem.Models.AssignmentStatus Status { get; set; }
        = EnglishLearningOnlineSystem.Models.AssignmentStatus.Published;

    public List<int> SelectedLessonIds { get; set; } = new();

    public List<CourseOptionViewModel> Courses { get; set; } = new();
    public List<AssignLessonItemViewModel> Lessons { get; set; } = new();
}

public class CourseOptionViewModel
{
    public int CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string GradeLevel { get; set; } = string.Empty;
}

public class AssignLessonItemViewModel
{
    public int LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public int XPReward { get; set; }
    public int VocabularyCount { get; set; }
    public int QuizCount { get; set; }
    public int MiniGameCount { get; set; }
    public bool IncludeVocabulary { get; set; } = true;
    public bool IncludeQuiz { get; set; } = true;
    public bool IncludeMiniGame { get; set; } = true;
    public List<int> SelectedVocabularyIds { get; set; } = new();
    public List<int> SelectedQuizIds { get; set; } = new();
    public List<int> SelectedMiniGameIds { get; set; } = new();
    public List<AssignmentVocabularyOptionViewModel> Vocabularies { get; set; } = new();
    public List<AssignmentQuizOptionViewModel> Quizzes { get; set; } = new();
    public List<AssignmentMiniGameOptionViewModel> MiniGames { get; set; } = new();
}

public class AssignmentVocabularyOptionViewModel
{
    public int VocabularyId { get; set; }
    public string Word { get; set; } = string.Empty;
    public string Meaning { get; set; } = string.Empty;
}

public class AssignmentQuizOptionViewModel
{
    public int QuizId { get; set; }
    public string Question { get; set; } = string.Empty;
    public string QuizType { get; set; } = string.Empty;
}

public class AssignmentMiniGameOptionViewModel
{
    public int GameId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string GameType { get; set; } = string.Empty;
}
