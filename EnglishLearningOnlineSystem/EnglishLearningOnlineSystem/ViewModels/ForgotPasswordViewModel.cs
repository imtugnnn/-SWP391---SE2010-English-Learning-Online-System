using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Please enter your email.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string Email { get; set; } = string.Empty;
}
