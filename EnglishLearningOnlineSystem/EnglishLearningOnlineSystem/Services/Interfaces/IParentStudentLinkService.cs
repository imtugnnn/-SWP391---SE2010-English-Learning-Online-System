using EnglishLearningOnlineSystem.Services.Models;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IParentStudentLinkService
{
    Task<UserServiceResult<List<LinkedStudentItem>>> GetLinkedStudentsAsync(int parentId);
    Task<UserServiceResult<VerifiedStudentInfo>> VerifyCodeAsync(string studentCode);
    Task<UserServiceResult<object>> LinkByCodeAsync(int parentId, string studentCode, string? relationship);
    Task<UserServiceResult<object>> UnlinkStudentAsync(int parentId, int linkId);
    Task<ParentDashboardViewModel> BuildDashboardAsync(int parentId, int? selectedStudentId);
}
