using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Interfaces;

public interface IClassService
{
    Task<TeacherClassDetailViewModel?> GetTeacherClassDetailAsync(int classId, int teacherId);
}