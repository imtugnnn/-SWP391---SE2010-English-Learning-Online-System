using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải chứa ít nhất 6 ký tự.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy xác nhận mật khẩu của bạn.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu không khớp.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
