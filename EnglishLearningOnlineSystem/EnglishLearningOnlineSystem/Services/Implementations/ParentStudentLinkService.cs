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
}
