using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class StudentDashboardService : IStudentDashboardService
{
    private readonly IStudentDashboardRepository _repo;
    private readonly IAdaptiveLearningService _adaptiveService;

    // XP thresholds per level — chỉnh lại cho khớp LevelConfig table của bạn
    private static readonly int[] XpThresholds = { 0, 100, 250, 500, 900, 1400, 2100, 3000, 4200, 6000 };

    public StudentDashboardService(
        IStudentDashboardRepository repo,
        IAdaptiveLearningService adaptiveService)
    {
        _repo = repo;
        _adaptiveService = adaptiveService;
    }

    public async Task<StudentDashboardViewModel?> GetDashboardAsync(int userId)
    {
        var profile = await _repo.GetProfileByUserIdAsync(userId);
        if (profile == null) return null;

        // Cập nhật streak + last active date
        await _repo.UpdateLastActiveDateAsync(profile.StudentId);

        // Reload sau khi update
        profile = (await _repo.GetProfileByUserIdAsync(userId))!;

        var assignedLessons = await _repo.GetCurrentWeekAssignmentsAsync(profile.StudentId);
        var recentProgress = await _repo.GetRecentProgressAsync(profile.StudentId);
        var todayMissions = await _repo.GetTodayMissionsAsync(profile.StudentId);
        var recentBadges = await _repo.GetRecentBadgesAsync(profile.StudentId);
        var totalCompleted = await _repo.GetTotalLessonsCompletedAsync(profile.StudentId);
        var recommendations = await _adaptiveService.GetSuggestionsAsync(profile.StudentId);

        return new StudentDashboardViewModel
        {
            Nickname = profile.Nickname ?? profile.User?.Username ?? "Student",
            AvatarUrl = profile.AvatarUrl ?? "/images/default-avatar.png",
            Level = profile.Level,
            XP = profile.XP,
            XPToNextLevel = GetXPToNextLevel(profile.Level, profile.XP),
            CurrentStreakDays = profile.CurrentStreakDays,
            LongestStreak = profile.CurrentStreakDays, // FIX: no LongestStreak in model, use CurrentStreak
            TotalLessonsCompleted = totalCompleted,
            TotalXPEarned = profile.XP,
            IsFirstLogin = profile.LastActiveDate == null,
            Recommendations = recommendations,

            AssignedLessons = assignedLessons.Select(wa => new AssignedLessonSummary
            {
                LessonId = wa.LessonId ?? 0,           // FIX: LessonId is int?
                Title = wa.Lesson?.Title ?? "—",
                Topic = wa.Lesson?.Topic ?? "",
                XPReward = wa.Lesson?.XPReward ?? 0,
                EstimatedMinutes = wa.Lesson?.EstimatedMinutes ?? 0,
                DueDate = wa.DueDate,                  // FIX: DueDate is already DateTime
                CompletionStatus = GetLessonStatus(recentProgress, wa.LessonId ?? 0)
            }).ToList(),

            RecentProgress = recentProgress.Select(p => new RecentProgressSummary
            {
                LessonTitle = p.Lesson?.Title ?? "—",
                QuizScore = p.QuizScore,
                XPEarned = p.XPEarned,
                CompletedAt = p.CompletedAt ?? DateTime.MinValue,
                CompletionStatus = p.CompletionStatus    // FIX: already a string, no .ToString() needed
            }).ToList(),

            DailyMissions = todayMissions.Select(sm => new DailyMissionSummary
            {
                // FIX: nav property is DailyMission, not Mission
                Description = sm.DailyMission?.Description ?? "",
                Type = sm.DailyMission?.Type ?? "",
                CurrentValue = sm.CurrentValue,
                TargetValue = sm.DailyMission?.TargetValue ?? 1,
                XPReward = sm.DailyMission?.XPReward ?? 0,
                IsCompleted = sm.IsCompleted
            }).ToList(),

            RecentBadges = recentBadges.Select(sb => new BadgeSummary
            {
                BadgeName = sb.Badge?.BadgeName ?? "",
                IconUrl = sb.Badge?.IconUrl ?? "",
                EarnedAt = sb.EarnedAt
            }).ToList()
        };
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static string GetLessonStatus(List<Progress> progresses, int lessonId)
    {
        var match = progresses.FirstOrDefault(p => p.LessonId == lessonId);
        return match?.CompletionStatus ?? "NOT_STARTED";
    }

    private static int GetXPToNextLevel(int currentLevel, int currentXP)
    {
        if (currentLevel >= XpThresholds.Length - 1) return 0;
        var needed = XpThresholds[currentLevel]; // XP needed to reach next level
        return Math.Max(0, needed - currentXP);
    }
}