using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IStudentGameProgressRepository
{
    Task AddAsync(StudentGameProgress progress);
    Task SaveChangesAsync();
}
