using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IParentStudentLinkRepository
{
    Task<List<ParentStudentLink>> GetByParentIdAsync(int parentId);
    Task<ParentStudentLink?> GetByIdAsync(int id);
    Task<bool> LinkExistsAsync(int parentId, int studentId);
    Task AddAsync(ParentStudentLink link);
    Task DeleteAsync(ParentStudentLink link);
}
