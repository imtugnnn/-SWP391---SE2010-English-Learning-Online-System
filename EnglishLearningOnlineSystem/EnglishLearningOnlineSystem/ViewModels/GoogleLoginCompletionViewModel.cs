using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EnglishLearningOnlineSystem.ViewModels;

public class GoogleLoginCompletionViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your username.")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please enter your password.")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please choose Student or Parent.")]
    public int RoleId { get; set; }

    [Required(ErrorMessage = "Please enter your birthdate.")]
    [DataType(DataType.Date)]
    public DateTime? BirthDate { get; set; }

    public List<SelectListItem> RoleOptions { get; set; } = new();
}
