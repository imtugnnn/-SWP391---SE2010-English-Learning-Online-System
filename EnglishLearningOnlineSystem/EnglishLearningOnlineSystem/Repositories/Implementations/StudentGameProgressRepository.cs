using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class StudentGameProgressRepository : IStudentGameProgressRepository
{
    private readonly AppDbContext _db;

    public StudentGameProgressRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(StudentGameProgress progress)
    {
        await _db.StudentGameProgresses!.AddAsync(progress);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
