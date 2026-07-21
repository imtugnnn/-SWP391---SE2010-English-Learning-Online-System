//Created by TungDPL
//Last update: 7/21/2026
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels
{
    public class UserCreateViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        //BR15: Password must contain at least 6 characters.
        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn role.")]
        public int RoleId { get; set; }
    }
}
