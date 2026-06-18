using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels
{
    public class ParentLinkPageViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mã học sinh hoặc mã mời.")]
        [MaxLength(20)]
        public string StudentCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Relationship { get; set; }

        public VerifiedStudentInfo? VerifiedStudent { get; set; }

        public List<LinkedStudentItem> LinkedStudents { get; set; } = new();
    }

    public class VerifiedStudentInfo
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public int Level { get; set; }
        public string? GradeLevel { get; set; }
        public string? ClassName { get; set; }
    }

    public class LinkedStudentItem
    {
        public int LinkId { get; set; }
        public int StudentId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Nickname { get; set; }
        public string? StudentCode { get; set; }
        public int Level { get; set; }
        public int XP { get; set; }
        public int CurrentStreakDays { get; set; }
        public string? Relationship { get; set; }
        public DateTime LinkedAt { get; set; }
    }
}
