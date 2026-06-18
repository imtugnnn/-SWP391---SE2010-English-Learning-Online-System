using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class ParentStudentLinkRepository : IParentStudentLinkRepository
{
    private readonly AppDbContext _context;

    public ParentStudentLinkRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<ParentStudentLink>> GetByParentIdAsync(int parentId)
    {
        return _context.ParentStudentLinks
            .Include(l => l.Student)
                .ThenInclude(s => s.User)
            .Where(l => l.ParentId == parentId)
            .OrderByDescending(l => l.LinkedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<ParentStudentLink?> GetByIdAsync(int id)
    {
        return _context.ParentStudentLinks
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public Task<bool> LinkExistsAsync(int parentId, int studentId)
    {
        return _context.ParentStudentLinks
            .AnyAsync(l => l.ParentId == parentId && l.StudentId == studentId);
    }

    public async Task AddAsync(ParentStudentLink link)
    {
        _context.ParentStudentLinks.Add(link);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(ParentStudentLink link)
    {
        _context.ParentStudentLinks.Remove(link);
        await _context.SaveChangesAsync();
    }
}
