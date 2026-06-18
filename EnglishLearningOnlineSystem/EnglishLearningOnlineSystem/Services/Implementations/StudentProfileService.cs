using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class StudentProfileService : IStudentProfileService
{
    private readonly IStudentProfileRepository _repo;

    public StudentProfileService(IStudentProfileRepository repo)
    {
        _repo = repo;
    }

    public async Task<StudentProfileViewModel?> GetProfileAsync(int userId)
    {
        var (profile, user) = await _repo.GetByUserIdAsync(userId);
        if (profile == null || user == null) return null;

        return new StudentProfileViewModel
        {
            Username = user.Username,
            Email = user.Email,
            BirthDate = user.BirthDate,
            Nickname = profile.Nickname ?? user.Username,
            AvatarUrl = profile.AvatarUrl ?? "/images/default-avatar.png",
            StudentCode = profile.StudentCode,
            Level = profile.Level,
            XP = profile.XP,
            CurrentStreakDays = profile.CurrentStreakDays,
            NewNickname = profile.Nickname ?? user.Username
        };
    }

    public async Task<bool> UpdateProfileAsync(int userId, string nickname, IFormFile? avatarFile, IWebHostEnvironment env)
    {
        string? savedAvatarUrl = null;

        if (avatarFile != null && avatarFile.Length > 0)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var ext = Path.GetExtension(avatarFile.FileName).ToLower();
            if (!allowed.Contains(ext)) return false;

            var uploadDir = Path.Combine(env.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{userId}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(uploadDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await avatarFile.CopyToAsync(stream);

            savedAvatarUrl = $"/uploads/avatars/{fileName}";
        }

        return await _repo.UpdateProfileAsync(userId, nickname, savedAvatarUrl);
    }
}
