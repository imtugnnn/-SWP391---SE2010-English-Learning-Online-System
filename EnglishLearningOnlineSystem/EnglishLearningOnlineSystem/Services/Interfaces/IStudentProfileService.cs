using EnglishLearningOnlineSystem.ViewModels;
using Microsoft.AspNetCore.Http;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IStudentProfileService
{
    Task<StudentProfileViewModel?> GetProfileAsync(int userId);
    Task<bool> UpdateProfileAsync(int userId, string nickname, IFormFile? avatarFile, IWebHostEnvironment env);
}
