using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class StudentProfileRepository : IStudentProfileRepository
{
    private readonly AppDbContext _db;

    public StudentProfileRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<(StudentProfile? profile, User? user)> GetByUserIdAsync(int userId)
    {
        var profile = await _db.StudentProfiles!
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.StudentId == userId);

        return (profile, profile?.User);
    }

    public async Task<bool> UpdateProfileAsync(
        int userId,
        string fullName,
        string nickname,
        string? avatarUrl)
    {
        var profile = await _db.StudentProfiles!.FindAsync(userId);
        if (profile == null) return false;

        profile.FullName = fullName;
        profile.Nickname = nickname;
        if (avatarUrl != null)
            profile.AvatarUrl = avatarUrl;

        await _db.SaveChangesAsync();
        return true;
    }
}
