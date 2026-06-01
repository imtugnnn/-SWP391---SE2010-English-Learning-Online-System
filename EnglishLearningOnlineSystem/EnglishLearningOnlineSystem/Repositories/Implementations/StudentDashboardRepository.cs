using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class StudentDashboardRepository : IStudentDashboardRepository
{
    private readonly AppDbContext _db;

    public StudentDashboardRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<StudentProfile?> GetProfileByUserIdAsync(int userId)
    {
        return await _db.StudentProfiles!
            .Include(sp => sp.User)
            .FirstOrDefaultAsync(sp => sp.StudentId == userId);
    }

    public async Task<List<WeeklyAssignment>> GetCurrentWeekAssignmentsAsync(int studentId)
    {
        var profile = await _db.StudentProfiles!
            .AsNoTracking()
            .FirstOrDefaultAsync(sp => sp.StudentId == studentId);

        if (profile == null) return new List<WeeklyAssignment>();

        // FIX: DueDate is DateTime, compare with DateTime.Today
        return await _db.WeeklyAssignments!
            .Include(wa => wa.Lesson)
            .Where(wa => wa.IsVisible && wa.DueDate >= DateTime.Today)
            .OrderBy(wa => wa.DueDate)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<Progress>> GetRecentProgressAsync(int studentId, int take = 5)
    {
        return await _db.Progresses!
            .Include(p => p.Lesson)
            .Where(p => p.StudentId == studentId && p.IsBestAttempt)
            .OrderByDescending(p => p.CompletedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<StudentMission>> GetTodayMissionsAsync(int studentId)
    {
        // FIX: StudentMission.Date is DateTime, compare with DateTime.Today.Date
        var today = DateTime.Today;

        return await _db.StudentMissions!
            .Include(sm => sm.DailyMission)   // FIX: nav property is DailyMission, not Mission
            .Where(sm => sm.StudentId == studentId && sm.Date.Date == today)
            .ToListAsync();
    }

    public async Task<List<StudentBadge>> GetRecentBadgesAsync(int studentId, int take = 3)
    {
        return await _db.StudentBadges!
            .Include(sb => sb.Badge)
            .Where(sb => sb.StudentId == studentId)
            .OrderByDescending(sb => sb.EarnedAt)
            .Take(take)
            .ToListAsync();
    }

    public async Task<int> GetTotalLessonsCompletedAsync(int studentId)
    {
        // FIX: CompletionStatus is string, not enum
        return await _db.Progresses!
            .Where(p => p.StudentId == studentId
                     && p.CompletionStatus == "Completed"
                     && p.IsBestAttempt)
            .CountAsync();
    }

    public async Task UpdateLastActiveDateAsync(int studentId)
    {
        var profile = await _db.StudentProfiles!.FindAsync(studentId);
        if (profile == null) return;

        var today = DateTime.Today;

        // FIX: StudentProfile has no LastStreakDate/LongestStreak
        // Use LastActiveDate for streak logic instead
        if (profile.LastActiveDate == null)
        {
            profile.CurrentStreakDays = 1;
        }
        else
        {
            var daysSinceLast = (today - profile.LastActiveDate.Value.Date).Days;

            if (daysSinceLast == 1)
            {
                profile.CurrentStreakDays++;
            }
            else if (daysSinceLast > 1)
            {
                profile.CurrentStreakDays = 1;
            }
            // daysSinceLast == 0 → already updated today, do nothing
        }

        profile.LastActiveDate = today;

        await _db.SaveChangesAsync();
    }
}