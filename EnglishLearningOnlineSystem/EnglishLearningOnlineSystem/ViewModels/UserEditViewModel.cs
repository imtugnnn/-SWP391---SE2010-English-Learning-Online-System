using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels
{
    public class UserEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Cho phép để trống => không đổi password
        public string? Password { get; set; }

        public DateTime? BirthDate { get; set; }

        public bool IsActive { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn role.")]
        public int RoleId { get; set; }
    }
}
