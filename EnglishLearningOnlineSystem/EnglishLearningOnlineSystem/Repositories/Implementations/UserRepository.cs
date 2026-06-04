using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> FindByEmailAsync(string email)
    {
        return _context.Users.FirstOrDefaultAsync(user => user.Email == email);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        return _context.Users.AnyAsync(user => user.Username == username);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        return _context.Users.AnyAsync(user => user.Email == email);
    }

    public async Task AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }

    public Task<List<User>> GetAllAsync()
    {
        return _context.Users
            .Include(u => u.Role)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }

}
