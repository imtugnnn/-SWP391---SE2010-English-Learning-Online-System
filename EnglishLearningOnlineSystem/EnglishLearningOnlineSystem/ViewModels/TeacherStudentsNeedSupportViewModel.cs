namespace EnglishLearningOnlineSystem.ViewModels;

public class TeacherStudentsNeedSupportViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public string ClassFilter { get; set; } = "all";
    public string ReasonFilter { get; set; } = "all";
    public string SortBy { get; set; } = "risk";

    public int TotalNeedSupport { get; set; }
    public int LowScoreCount { get; set; }
    public int OverdueCount { get; set; }
    public int InactiveCount { get; set; }
    public int NotStartedCount { get; set; }

    public List<TeacherSupportClassOptionViewModel> Classes { get; set; } = new();
    public List<TeacherSupportStudentItemViewModel> Students { get; set; } = new();
}

public class TeacherSupportClassOptionViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
}

public class TeacherSupportStudentItemViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public double? AverageQuizScore { get; set; }
    public int OverdueLessonCount { get; set; }
    public int NotStartedLessonCount { get; set; }
    public DateTime? LastActiveDate { get; set; }
    public bool IsInactive { get; set; }
    public bool HasLowScore { get; set; }
    public bool HasOverdueAssignments { get; set; }
    public bool HasNotStartedLessons { get; set; }
    public int RiskScore { get; set; }

    public List<string> Reasons { get; set; } = new();

    public string AverageQuizScoreText => AverageQuizScore.HasValue
        ? $"{AverageQuizScore.Value:0.#}%"
        : "-";

    public string LastActiveText => LastActiveDate.HasValue
        ? LastActiveDate.Value.ToString("dd/MM/yyyy")
        : "Chưa hoạt động";
}
