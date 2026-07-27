//Create by TungDPL
//Create at 7/28/2026
using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Repositories.Interfaces;

public interface IAcademicYearRepository
{
    Task<List<AcademicYear>> GetAcademicYearsAsync();
    Task<AcademicYear?> GetAcademicYearByIdAsync(int academicYearId);
    Task<List<AcademicYear>> GetActiveAcademicYearsAsync();
    Task<bool> AcademicYearLabelExistsAsync(string yearLabel, int? excludeAcademicYearId = null);
    Task<bool> HasActiveAcademicYearAsync();
    Task AddAcademicYearAsync(AcademicYear academicYear);

    Task<List<Class>> GetClassesByAcademicYearIdAsync(int academicYearId);
    Task<Class?> GetClassByIdAsync(int classId, bool includeAcademicYear = false, bool includeTeacher = false, bool includeEnrollments = false);
    Task<bool> ClassNameExistsAsync(int academicYearId, string className, int? excludeClassId = null);
    Task AddClassAsync(Class classEntity);

    Task<List<User>> GetTeachersAsync();
    Task<User?> GetUserByEmailAsync(string email, bool includeRole = false);
    Task<List<User>> GetUsersByEmailsAsync(IEnumerable<string> emails);

    Task<List<ClassEnrollment>> GetClassEnrollmentsAsync(int classId);
    Task<ClassEnrollment?> GetEnrollmentByClassAndStudentAsync(int classId, int studentId);
    Task<List<ClassEnrollment>> GetOtherClassEnrollmentsByEmailsAsync(int academicYearId, int classId, IEnumerable<string> emails);
    Task AddClassEnrollmentsAsync(IEnumerable<ClassEnrollment> enrollments);
    Task RemoveClassEnrollmentAsync(ClassEnrollment enrollment);

    Task SaveChangesAsync();
}
