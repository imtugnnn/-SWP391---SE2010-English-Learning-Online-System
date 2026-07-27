//Create by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class AcademicYearRepository : IAcademicYearRepository
{
    private readonly AppDbContext _context;

    public AcademicYearRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AcademicYear>> GetAcademicYearsAsync()
    {
        return await _context.AcademicYears!
            .AsNoTracking()
            .Include(y => y.Classes)
            .ToListAsync();
    }

    public async Task<AcademicYear?> GetAcademicYearByIdAsync(int academicYearId)
    {
        return await _context.AcademicYears!
            .FirstOrDefaultAsync(y => y.AcademicYearId == academicYearId);
    }

    public async Task<List<AcademicYear>> GetActiveAcademicYearsAsync()
    {
        return await _context.AcademicYears!
            .Where(y => y.IsActive)
            .ToListAsync();
    }

    public async Task<bool> AcademicYearLabelExistsAsync(string yearLabel, int? excludeAcademicYearId = null)
    {
        IQueryable<AcademicYear> query = _context.AcademicYears!.AsNoTracking().Where(y => y.YearLabel == yearLabel);

        if (excludeAcademicYearId.HasValue)
        {
            query = query.Where(y => y.AcademicYearId != excludeAcademicYearId.Value);
        }

        return await query.AnyAsync();
    }

    public Task<bool> HasActiveAcademicYearAsync()
    {
        return _context.AcademicYears!.AnyAsync(y => y.IsActive);
    }

    public Task AddAcademicYearAsync(AcademicYear academicYear)
    {
        _context.AcademicYears!.Add(academicYear);
        return Task.CompletedTask;
    }

    public async Task<List<Class>> GetClassesByAcademicYearIdAsync(int academicYearId)
    {
        return await _context.Classes!
            .AsNoTracking()
            .Include(c => c.Course)
            .Include(c => c.Teacher)
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .Where(c => c.AcademicYearId == academicYearId)
            .OrderBy(c => c.ClassName)
            .ToListAsync();
    }

    public async Task<Class?> GetClassByIdAsync(int classId, bool includeAcademicYear = false, bool includeTeacher = false, bool includeEnrollments = false)
    {
        IQueryable<Class> query = _context.Classes!;

        if (includeAcademicYear)
        {
            query = query.Include(c => c.AcademicYear);
        }

        if (includeTeacher)
        {
            query = query.Include(c => c.Teacher);
        }

        if (includeEnrollments)
        {
            query = query
                .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student);
        }

        return await query.FirstOrDefaultAsync(c => c.ClassId == classId);
    }

    public async Task<bool> ClassNameExistsAsync(int academicYearId, string className, int? excludeClassId = null)
    {
        IQueryable<Class> query = _context.Classes!
            .AsNoTracking()
            .Where(c => c.AcademicYearId == academicYearId && c.ClassName == className && !c.IsDeleted);

        if (excludeClassId.HasValue)
        {
            query = query.Where(c => c.ClassId != excludeClassId.Value);
        }

        return await query.AnyAsync();
    }

    public Task AddClassAsync(Class classEntity)
    {
        _context.Classes!.Add(classEntity);
        return Task.CompletedTask;
    }

    public async Task<List<User>> GetTeachersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.Role != null && u.Role.Name == "Teacher")
            .OrderBy(u => u.Username)
            .ToListAsync();
    }

    public async Task<User?> GetUserByEmailAsync(string email, bool includeRole = false)
    {
        IQueryable<User> query = _context.Users;

        if (includeRole)
        {
            query = query.Include(u => u.Role);
        }

        return await query.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<List<User>> GetUsersByEmailsAsync(IEnumerable<string> emails)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => emails.Contains(u.Email))
            .ToListAsync();
    }

    public async Task<List<ClassEnrollment>> GetClassEnrollmentsAsync(int classId)
    {
        return await _context.ClassEnrollments!
            .AsNoTracking()
            .Include(e => e.Student)
            .Where(e => e.ClassId == classId)
            .ToListAsync();
    }

    public async Task<ClassEnrollment?> GetEnrollmentByClassAndStudentAsync(int classId, int studentId)
    {
        return await _context.ClassEnrollments!
            .FirstOrDefaultAsync(e => e.ClassId == classId && e.StudentId == studentId);
    }

    public async Task<List<ClassEnrollment>> GetOtherClassEnrollmentsByEmailsAsync(int academicYearId, int classId, IEnumerable<string> emails)
    {
        return await _context.ClassEnrollments!
            .AsNoTracking()
            .Include(e => e.Class)
            .Include(e => e.Student)
            .Where(e => e.Class.AcademicYearId == academicYearId
                        && !e.Class.IsDeleted
                        && e.ClassId != classId
                        && emails.Contains(e.Student.Email))
            .ToListAsync();
    }

    public Task AddClassEnrollmentsAsync(IEnumerable<ClassEnrollment> enrollments)
    {
        _context.ClassEnrollments!.AddRange(enrollments);
        return Task.CompletedTask;
    }

    public Task RemoveClassEnrollmentAsync(ClassEnrollment enrollment)
    {
        _context.ClassEnrollments!.Remove(enrollment);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
