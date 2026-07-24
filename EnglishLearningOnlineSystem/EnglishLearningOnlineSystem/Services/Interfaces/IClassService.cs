using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IClassService
{
    /// <summary>
    /// Lấy chi tiết lớp dành cho giáo viên được phân công.
    /// </summary>
    Task<TeacherClassDetailViewModel?> GetTeacherClassDetailAsync(int classId, int teacherId);

}
