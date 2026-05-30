using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; 
    public DateTime? BirthDate { get; set; }
    public bool IsActive { get; set; } = true;
    public int RoleId { get; set; }   // khóa ngoại
    public Role? Role { get; set; }   // navigation property

    // Add this navigation property for the classes the user teaches
    public ICollection<Class> TaughtClasses { get; set; }

    // Add this property to fix the error:
    public ICollection<TeacherFeedback> GivenFeedbacks { get; set; }
}
