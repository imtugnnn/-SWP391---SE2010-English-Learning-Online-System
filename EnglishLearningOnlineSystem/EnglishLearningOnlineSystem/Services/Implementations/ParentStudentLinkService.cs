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
}
