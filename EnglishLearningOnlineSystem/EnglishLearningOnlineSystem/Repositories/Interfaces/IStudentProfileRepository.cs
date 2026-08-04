using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IStudentProfileRepository
{
    Task<(StudentProfile? profile, User? user)> GetByUserIdAsync(int userId);
    Task<bool> UpdateProfileAsync(int userId, string fullName, string nickname, string? avatarUrl);
}
