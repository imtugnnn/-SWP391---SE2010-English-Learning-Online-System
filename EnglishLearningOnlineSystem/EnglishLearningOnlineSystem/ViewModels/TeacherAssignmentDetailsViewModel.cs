using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherAssignmentDetailsViewModel
{
    public int AssignmentId { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int AssignedStudentCount { get; set; }
    public int CompletedStudentCount { get; set; }
    public int InProgressStudentCount { get; set; }
    public int NotStartedStudentCount { get; set; }
    public int CompletedLateCount { get; set; }
    public bool CanEdit { get; set; }
    public bool CanCancel { get; set; }
    public bool CanDelete { get; set; }
    public bool CanArchive { get; set; }
    public List<TeacherAssignmentActivityViewModel> Activities { get; set; } = new();
    public List<TeacherAssignmentStudentCompletionViewModel> Students { get; set; } = new();
}

public class TeacherAssignmentActivityViewModel
{
    public string ActivityType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public bool IsRequired { get; set; }
}

public class TeacherAssignmentStudentCompletionViewModel
{
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CompletionStatus { get; set; } = "NotStarted";
    public int CompletedActivityCount { get; set; }
    public int RequiredActivityCount { get; set; }
    public int? QuizScore { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsCompletedLate { get; set; }
}

public class EditTeacherAssignmentViewModel
{
    public int AssignmentId { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public string Status { get; set; } = string.Empty;
    public bool IncludeVocabulary { get; set; }
    public bool IncludeQuiz { get; set; }
    public bool IncludeMiniGame { get; set; }
    public List<int> SelectedVocabularyIds { get; set; } = new();
    public List<int> SelectedQuizIds { get; set; } = new();
    public List<int> SelectedMiniGameIds { get; set; } = new();
    public List<AssignmentVocabularyOptionViewModel> Vocabularies { get; set; } = new();
    public List<AssignmentQuizOptionViewModel> Quizzes { get; set; } = new();
    public List<AssignmentMiniGameOptionViewModel> MiniGames { get; set; } = new();
}
