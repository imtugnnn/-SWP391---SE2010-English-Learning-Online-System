using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.SeedData;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync(x => x.Email == "admin@english.com"))
            return;

        var admin = new User
        {
            Username = "admin",
            Email = "admin@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 2
        };

        context.Users.Add(admin);

        await context.SaveChangesAsync();

        // Seed student test account
        var studentUser = new User
        {
            Username = "student01",
            Email = "student@english.com",
            Password = BCrypt.Net.BCrypt.HashPassword("123456"),
            IsActive = true,
            RoleId = 1
        };

        context.Users.Add(studentUser);
        await context.SaveChangesAsync();

        context.StudentProfiles!.Add(new StudentProfile
        {
            StudentId = studentUser.Id,
            Nickname = "Student 01",
            AvatarUrl = "/images/default-avatar.png",
            Level = 1,
            XP = 0,
            CurrentStreakDays = 0,
            LastActiveDate = null
        });

        await context.SaveChangesAsync();
    }
}