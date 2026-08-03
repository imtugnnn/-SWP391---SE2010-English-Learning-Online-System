using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class AssignmentProgressRepository : IAssignmentProgressRepository
{
    private readonly AppDbContext _db;

    public AssignmentProgressRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<WeeklyAssignment?> GetAccessibleAssignmentAsync(int assignmentId, int studentId)
    {
        // Quyền truy cập được kiểm tra tại nguồn dữ liệu để không thể đổi assignmentId trên URL.
        return _db.WeeklyAssignments!
            .Include(x => x.Vocabularies)
            .Include(x => x.Quizzes)
            .Include(x => x.MiniGames)
            .FirstOrDefaultAsync(x =>
                x.AssignmentId == assignmentId &&
                x.Status == AssignmentStatus.Published &&
                x.IsVisible &&
                x.ClassId.HasValue &&
                _db.ClassEnrollments!.Any(e =>
                    e.ClassId == x.ClassId.Value && e.StudentId == studentId));
    }

    public Task<AssignmentProgress?> GetProgressAsync(int assignmentId, int studentId)
    {
        return _db.AssignmentProgresses
            .FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.StudentId == studentId);
    }

    public Task<List<AssignmentActivityProgress>> GetActivityProgressesAsync(int assignmentId, int studentId)
    {
        return _db.AssignmentActivityProgresses
            .Where(x => x.AssignmentId == assignmentId && x.StudentId == studentId)
            .ToListAsync();
    }

    public Task AddProgressAsync(AssignmentProgress progress)
    {
        return _db.AssignmentProgresses.AddAsync(progress).AsTask();
    }

    public Task AddActivityProgressAsync(AssignmentActivityProgress progress)
    {
        return _db.AssignmentActivityProgresses.AddAsync(progress).AsTask();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
