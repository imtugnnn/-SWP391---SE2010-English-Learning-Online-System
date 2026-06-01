using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class RoleRepository : IRoleRepository
{
    private readonly AppDbContext _context;

    public RoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<Role?> FindRegistrationRoleAsync(int roleId)
    {
        return _context.Roles.FirstOrDefaultAsync(role =>
            role.Id == roleId &&
            (role.Name == "Student" || role.Name == "Parent"));
    }

    public Task<List<Role>> GetRegistrationRolesAsync()
    {
        return _context.Roles
            .Where(role => role.Name == "Student" || role.Name == "Parent")
            .OrderBy(role => role.Name)
            .ToListAsync();
    }
}
