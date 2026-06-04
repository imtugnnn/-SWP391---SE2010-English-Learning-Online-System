using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task<StudentProfile?> FindStudentProfileAsync(int userId);
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task AddStudentProfileAsync(StudentProfile studentProfile);
}
