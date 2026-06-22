using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class ParentStudentLinkService : IParentStudentLinkService
{
    private readonly IParentStudentLinkRepository _linkRepo;
    private readonly IUserRepository _userRepo;

    public ParentStudentLinkService(
        IParentStudentLinkRepository linkRepo,
        IUserRepository userRepo)
    {
        _linkRepo = linkRepo;
        _userRepo = userRepo;
    }

    public async Task<UserServiceResult<List<LinkedStudentItem>>> GetLinkedStudentsAsync(int parentId)
    {
        var links = await _linkRepo.GetByParentIdAsync(parentId);

        var items = links.Select(l => new LinkedStudentItem
        {
            LinkId = l.Id,
            StudentId = l.StudentId,
            Username = l.Student?.User?.Username ?? string.Empty,
            Email = l.Student?.User?.Email ?? string.Empty,
            Nickname = l.Student?.Nickname,
            StudentCode = l.Student?.StudentCode,
            Level = l.Student?.Level ?? 0,
            XP = l.Student?.XP ?? 0,
            CurrentStreakDays = l.Student?.CurrentStreakDays ?? 0,
            Relationship = l.Relationship,
            LinkedAt = l.LinkedAt
        }).ToList();

        return UserServiceResult<List<LinkedStudentItem>>.Ok(items);
    }

    public async Task<UserServiceResult<VerifiedStudentInfo>> VerifyCodeAsync(string studentCode)
    {
        var code = (studentCode ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(code))
            return UserServiceResult<VerifiedStudentInfo>.Fail("Vui lòng nhập mã học sinh hoặc mã mời.");

        var profile = await _userRepo.FindStudentProfileByCodeAsync(code);
        if (profile == null)
            return UserServiceResult<VerifiedStudentInfo>.Fail("Mã không hợp lệ. Vui lòng kiểm tra lại mã học sinh hoặc mã mời.");

        if (profile.User == null)
            return UserServiceResult<VerifiedStudentInfo>.Fail("Không tìm thấy thông tin học sinh tương ứng với mã này.");

        var info = new VerifiedStudentInfo
        {
            StudentId = profile.StudentId,
            StudentCode = profile.StudentCode ?? code,
            Username = profile.User.Username,
            DisplayName = string.IsNullOrWhiteSpace(profile.Nickname) ? profile.User.Username : profile.Nickname,
            AvatarUrl = profile.AvatarUrl,
            Level = profile.Level
        };

        return UserServiceResult<VerifiedStudentInfo>.Ok(info);
    }

    public async Task<UserServiceResult<object>> LinkByCodeAsync(int parentId, string studentCode, string? relationship)
    {
        var verify = await VerifyCodeAsync(studentCode);
        if (!verify.Succeeded || verify.Data == null)
            return UserServiceResult<object>.Fail(verify.ErrorMessage ?? "Mã không hợp lệ.");

        var studentId = verify.Data.StudentId;

        if (studentId == parentId)
            return UserServiceResult<object>.Fail("Bạn không thể liên kết với chính mình.");

        if (await _linkRepo.LinkExistsAsync(parentId, studentId))
            return UserServiceResult<object>.Fail("Học sinh này đã được liên kết với tài khoản của bạn.");

        var link = new ParentStudentLink
        {
            ParentId = parentId,
            StudentId = studentId,
            Relationship = string.IsNullOrWhiteSpace(relationship) ? null : relationship.Trim(),
            LinkedAt = DateTime.UtcNow
        };

        await _linkRepo.AddAsync(link);
        return UserServiceResult<object>.Ok(null);
    }

    public async Task<UserServiceResult<object>> UnlinkStudentAsync(int parentId, int linkId)
    {
        var link = await _linkRepo.GetByIdAsync(linkId);
        if (link == null)
            return UserServiceResult<object>.Fail("Không tìm thấy liên kết.");

        if (link.ParentId != parentId)
            return UserServiceResult<object>.Fail("Bạn không có quyền hủy liên kết này.");

        await _linkRepo.DeleteAsync(link);
        return UserServiceResult<object>.Ok(null);
    }

    public async Task<ParentDashboardViewModel> BuildDashboardAsync(int parentId, int? selectedStudentId)
    {
        var vm = new ParentDashboardViewModel();

        try
        {
            var links = await _linkRepo.GetByParentIdAsync(parentId);

            if (!links.Any())
            {
                vm.HasLinkedChildren = false;
                return vm;
            }

            vm.HasLinkedChildren = true;

            var targetId = selectedStudentId ?? links.First().StudentId;
            if (links.All(l => l.StudentId != targetId))
            {
                targetId = links.First().StudentId;
            }

            vm.SelectedStudentId = targetId;
            vm.Children = links.Select(l => new ChildOption
            {
                StudentId = l.StudentId,
                DisplayName = string.IsNullOrWhiteSpace(l.Student?.Nickname)
                    ? l.Student?.User?.Username ?? "Học sinh"
                    : l.Student!.Nickname,
                AvatarUrl = l.Student?.AvatarUrl,
                Relationship = l.Relationship,
                IsSelected = l.StudentId == targetId
            }).ToList();

            var profile = await _linkRepo.GetLinkedStudentProfileAsync(parentId, targetId);
            if (profile == null)
            {
                vm.HasLinkedChildren = false;
                return vm;
            }

            var lessonsCompleted = await _linkRepo.CountCompletedLessonsAsync(targetId);
            var badges = await _linkRepo.CountBadgesAsync(targetId);
            var avgScore = await _linkRepo.GetAverageQuizScoreAsync(targetId);
            var recentProgress = await _linkRepo.GetRecentProgressAsync(targetId, 5);
            var upcoming = await _linkRepo.GetUpcomingAssignmentsAsync(5);
            var recentBadges = await _linkRepo.GetRecentBadgesAsync(targetId, 3);

            var overview = new ChildLearningOverview
            {
                DisplayName = string.IsNullOrWhiteSpace(profile.Nickname)
                    ? profile.User?.Username ?? "Học sinh"
                    : profile.Nickname,
                Username = profile.User?.Username ?? string.Empty,
                AvatarUrl = profile.AvatarUrl,
                StudentCode = profile.StudentCode,
                Level = profile.Level,
                XP = profile.XP,
                CurrentStreakDays = profile.CurrentStreakDays,
                LastActiveDate = profile.LastActiveDate,
                LessonsCompleted = lessonsCompleted,
                BadgesEarned = badges,
                AverageQuizScore = avgScore.HasValue ? (int)Math.Round(avgScore.Value) : 0,
                RecentActivities = recentProgress.Select(p => new ParentRecentActivity
                {
                    LessonTitle = p.Lesson?.Title ?? "Bài học",
                    QuizScore = p.QuizScore,
                    XPEarned = p.XPEarned,
                    CompletionStatus = p.CompletionStatus,
                    CompletedAt = p.CompletedAt
                }).ToList(),
                UpcomingTasks = upcoming.Select(wa => new ParentUpcomingTask
                {
                    LessonTitle = wa.Lesson?.Title ?? "Bài học",
                    Topic = wa.Lesson?.Topic ?? string.Empty,
                    XPReward = wa.Lesson?.XPReward ?? 0,
                    DueDate = wa.DueDate
                }).ToList(),
                RecentBadges = recentBadges.Select(sb => new ParentBadgeItem
                {
                    BadgeName = sb.Badge?.BadgeName ?? "Huy hiệu",
                    IconUrl = sb.Badge?.IconUrl,
                    EarnedAt = sb.EarnedAt
                }).ToList()
            };

            overview.HasLearningData = lessonsCompleted > 0
                || avgScore.HasValue
                || overview.RecentActivities.Any()
                || profile.XP > 0;

            vm.Overview = overview;
            return vm;
        }
        catch
        {
            vm.LoadFailed = true;
            return vm;
        }
    }

    public async Task<ParentReportViewModel> BuildReportAsync(int parentId, int? selectedStudentId, string? period, DateTime? fromDate, DateTime? toDate)
    {
        var vm = new ParentReportViewModel();

        try
        {
            var links = await _linkRepo.GetByParentIdAsync(parentId);

            if (!links.Any())
            {
                vm.HasLinkedChildren = false;
                return vm;
            }

            vm.HasLinkedChildren = true;

            var targetId = selectedStudentId ?? links.First().StudentId;
            if (links.All(l => l.StudentId != targetId))
            {
                targetId = links.First().StudentId;
            }

            vm.SelectedStudentId = targetId;
            vm.Children = links.Select(l => new ChildOption
            {
                StudentId = l.StudentId,
                DisplayName = string.IsNullOrWhiteSpace(l.Student?.Nickname)
                    ? l.Student?.User?.Username ?? "Học sinh"
                    : l.Student!.Nickname,
                AvatarUrl = l.Student?.AvatarUrl,
                Relationship = l.Relationship,
                IsSelected = l.StudentId == targetId
            }).ToList();

            var profile = await _linkRepo.GetLinkedStudentProfileAsync(parentId, targetId);
            if (profile == null)
            {
                vm.HasLinkedChildren = false;
                return vm;
            }

            vm.ChildDisplayName = string.IsNullOrWhiteSpace(profile.Nickname)
                ? profile.User?.Username ?? "Học sinh"
                : profile.Nickname;
            vm.ChildAvatarUrl = profile.AvatarUrl;

            (vm.Period, vm.FromDate, vm.ToDate) = ResolvePeriod(period, fromDate, toDate);

            var attempts = await _linkRepo.GetQuizAttemptsInPeriodAsync(targetId, vm.FromDate, vm.ToDate);
            var progress = await _linkRepo.GetProgressInPeriodAsync(targetId, vm.FromDate, vm.ToDate);
            var feedbacks = await _linkRepo.GetFeedbacksAsync(targetId, 10);

            vm.LessonsCompleted = progress.Count(p => p.CompletionStatus == "Completed");
            vm.QuizzesTaken = attempts.Count;
            vm.AverageQuizScore = attempts.Any() ? (int)Math.Round(attempts.Average(a => (double)a.Score)) : 0;
            vm.XPEarnedInPeriod = progress.Sum(p => p.XPEarned);
            vm.TotalTimeSpentMinutes = (int)Math.Round(attempts.Sum(a => a.TimeSpentSec) / 60.0);

            vm.SkillProgress = attempts
                .Where(a => a.Lesson != null)
                .GroupBy(a => string.IsNullOrWhiteSpace(a.Lesson!.Topic) ? "Khác" : a.Lesson.Topic)
                .Select(g => new SkillProgressItem
                {
                    Topic = g.Key,
                    AverageScore = (int)Math.Round(g.Average(a => (double)a.Score)),
                    AttemptCount = g.Count()
                })
                .OrderByDescending(s => s.AverageScore)
                .ToList();

            vm.QuizResults = attempts.Select(a => new QuizResultItem
            {
                LessonTitle = a.Lesson?.Title ?? "Bài học",
                Topic = a.Lesson?.Topic ?? string.Empty,
                Score = a.Score,
                CorrectCount = a.CorrectCount,
                TotalQuestions = a.TotalQuestions,
                TimeSpentSec = a.TimeSpentSec,
                SubmittedAt = a.SubmittedAt
            }).ToList();

            vm.Feedbacks = feedbacks.Select(f => new TeacherFeedbackItem
            {
                Content = f.Content,
                TeacherName = f.Teacher?.Username ?? "Giáo viên",
                CreatedAt = f.CreateAt
            }).ToList();

            vm.HasReportData = vm.QuizzesTaken > 0
                || vm.LessonsCompleted > 0
                || vm.Feedbacks.Any();

            return vm;
        }
        catch
        {
            vm.LoadFailed = true;
            return vm;
        }
    }

    private static (string period, DateTime from, DateTime to) ResolvePeriod(string? period, DateTime? fromDate, DateTime? toDate)
    {
        var today = DateTime.Today;
        period = string.IsNullOrWhiteSpace(period) ? "week" : period.Trim().ToLower();

        switch (period)
        {
            case "month":
                return ("month", new DateTime(today.Year, today.Month, 1), today.AddDays(1));
            case "all":
                return ("all", new DateTime(2000, 1, 1), today.AddDays(1));
            case "custom":
                var from = fromDate?.Date ?? today.AddDays(-7);
                var to = (toDate?.Date ?? today).AddDays(1);
                if (to <= from) to = from.AddDays(1);
                return ("custom", from, to);
            case "week":
            default:
                var diff = (7 + (int)today.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var monday = today.AddDays(-diff);
                return ("week", monday, today.AddDays(1));
        }
    }
}
