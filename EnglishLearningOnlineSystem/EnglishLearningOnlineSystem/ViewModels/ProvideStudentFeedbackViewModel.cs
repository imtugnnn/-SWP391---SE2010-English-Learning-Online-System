using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class ProvideStudentFeedbackViewModel
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập nội dung phản hồi.")]
    [StringLength(1000, ErrorMessage = "Nội dung phản hồi không được vượt quá 1000 ký tự.")]
    public string Content { get; set; } = string.Empty;
}