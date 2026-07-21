//Create by TungDPL
//Last update: 7/17/2026
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

    public DateTime CreateAt { get; set; }
    public DateTime UpdateAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int AccessFailedCount { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }

    public ICollection<Class> TaughtClasses { get; set; } = new List<Class>();
    public ICollection<ClassEnrollment> ClassEnrollments { get; set; } = new List<ClassEnrollment>();
    public ICollection<TeacherFeedback> GivenFeedbacks { get; set; } = new List<TeacherFeedback>();
}
