//Create by TungDPL
//Last update: 7/21/2026
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class ResetPasswordViewModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    //BR15: Password must contain at least 6 characters.
    [Required(ErrorMessage = "Hãy nhập mật khẩu mới.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải chứa ít nhất 6 ký tự.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hãy xác nhận mật khẩu của bạn.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
