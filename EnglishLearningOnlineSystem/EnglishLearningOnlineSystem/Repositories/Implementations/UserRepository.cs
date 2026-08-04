//Create by TungDPL
//Last update: 7/21/2026
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.ViewModels.Admin;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    //Hàm kiểm tra email đăng nhập 
    public Task<User?> FindByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return _context.Users.FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail);
    }

    public Task<StudentProfile?> FindStudentProfileAsync(int userId)
    {
        return _context.StudentProfiles!.FirstOrDefaultAsync(profile => profile.StudentId == userId);
    }

    public Task<bool> UsernameExistsAsync(string username)
    {
        return _context.Users.AnyAsync(user => user.Username == username);
    }

    public Task<bool> EmailExistsAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLower();
        return _context.Users.AnyAsync(user => user.Email.ToLower() == normalizedEmail);
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
            .Include(u => u.StudentProfile)
            .Include(u => u.ClassEnrollments)
                .ThenInclude(ce => ce.Class)
                    .ThenInclude(c => c.AcademicYear)
            .Include(u => u.TaughtClasses)
                .ThenInclude(c => c.AcademicYear)
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<User?> GetByIdAsync(int id)
    {
        return _context.Users
            .Include(u => u.Role)
            .Include(u => u.StudentProfile)
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

    public async Task AddStudentProfileAsync(StudentProfile studentProfile)
    {
        _context.StudentProfiles!.Add(studentProfile);
        await _context.SaveChangesAsync();
    }

    public async Task<UserStatsViewModel> GetUserStatsAsync(DateTime thisMonthStart, DateTime lastMonthStart)
    {
        var query = _context.Users.AsNoTracking();
        return new UserStatsViewModel
        {
            TotalUsers = await query.CountAsync(),
            ActiveUsers = await query.CountAsync(u => u.IsActive),
            StudentCount = await query.CountAsync(u => u.Role!.Name == "Student"),
            TeacherCount = await query.CountAsync(u => u.Role!.Name == "Teacher"),
            ParentCount = await query.CountAsync(u => u.Role!.Name == "Parent"),
            ContentManagerCount = await query.CountAsync(u => u.Role!.Name == "Content Manager"),
            
            NewThisMonth = await query.CountAsync(u => u.CreateAt >= thisMonthStart),
            NewLastMonth = await query.CountAsync(u => u.CreateAt >= lastMonthStart && u.CreateAt < thisMonthStart),
            
            ActiveThisMonth = await query.CountAsync(u => u.IsActive && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= thisMonthStart),
            ActiveLastMonth = await query.CountAsync(u => u.IsActive && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= lastMonthStart && u.LastLoginAt.Value < thisMonthStart),
            
            StudentsThisMonth = await query.CountAsync(u => u.Role!.Name == "Student" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= thisMonthStart),
            StudentsLastMonth = await query.CountAsync(u => u.Role!.Name == "Student" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= lastMonthStart && u.LastLoginAt.Value < thisMonthStart),
            
            TeachersThisMonth = await query.CountAsync(u => u.Role!.Name == "Teacher" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= thisMonthStart),
            TeachersLastMonth = await query.CountAsync(u => u.Role!.Name == "Teacher" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= lastMonthStart && u.LastLoginAt.Value < thisMonthStart),
            
            ParentsThisMonth = await query.CountAsync(u => u.Role!.Name == "Parent" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= thisMonthStart),
            ParentsLastMonth = await query.CountAsync(u => u.Role!.Name == "Parent" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= lastMonthStart && u.LastLoginAt.Value < thisMonthStart),
            
            ContentThisMonth = await query.CountAsync(u => u.Role!.Name == "Content Manager" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= thisMonthStart),
            ContentLastMonth = await query.CountAsync(u => u.Role!.Name == "Content Manager" && u.LastLoginAt.HasValue && u.LastLoginAt.Value >= lastMonthStart && u.LastLoginAt.Value < thisMonthStart)
        };
    }
}
