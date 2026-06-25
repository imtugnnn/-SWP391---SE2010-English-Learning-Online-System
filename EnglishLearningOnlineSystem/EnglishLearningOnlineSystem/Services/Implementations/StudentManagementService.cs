using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using EnglishLearningOnlineSystem.Services.Interfaces;
using EnglishLearningOnlineSystem.ViewModels;

namespace EnglishLearningOnlineSystem.Services.Implementations;

public class StudentManagementService : IStudentManagementService
{
    private const int PageSize = 10;

    private readonly IClassRepository _classRepository;

    public StudentManagementService(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    public async Task<ManageStudentListViewModel?> GetManageStudentListAsync(
        int classId,
        int teacherId,
        string? keyword,
        string? status,
        string? sortBy,
        int page)
    {
        var classEntity = await ValidateTeacherAccessAsync(classId, teacherId);

        if (classEntity == null)
        {
            return null;
        }

        var enrollments = await _classRepository.GetStudentsByClassIdAsync(classId);

        var totalStudents = enrollments.Count;
        var activeStudents = enrollments.Count(e => e.Student.IsActive);
        var inactiveStudents = enrollments.Count(e => !e.Student.IsActive);

        var filteredEnrollments = ApplySearch(enrollments, keyword);
        filteredEnrollments = ApplyStatusFilter(filteredEnrollments, status);
        filteredEnrollments = ApplySorting(filteredEnrollments, sortBy);

        page = NormalizePage(page);

        var totalItems = filteredEnrollments.Count;
        var totalPages = CalculateTotalPages(totalItems, PageSize);

        if (totalPages > 0 && page > totalPages)
        {
            page = totalPages;
        }

        var pagedEnrollments = ApplyPagination(filteredEnrollments, page, PageSize);

        return BuildViewModel(
            classEntity,
            pagedEnrollments,
            keyword,
            status,
            sortBy,
            page,
            PageSize,
            totalItems,
            totalPages,
            totalStudents,
            activeStudents,
            inactiveStudents);
    }

    private async Task<Class?> ValidateTeacherAccessAsync(int classId, int teacherId)
    {
        var classEntity = await _classRepository.GetClassDetailByIdAsync(classId);

        if (classEntity == null)
        {
            return null;
        }

        if (classEntity.TeacherId != teacherId)
        {
            return null;
        }

        return classEntity;
    }

    private static List<ClassEnrollment> ApplySearch(
        List<ClassEnrollment> enrollments,
        string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
        {
            return enrollments;
        }

        var normalizedKeyword = keyword.Trim().ToLower();

        return enrollments
            .Where(e =>
                e.Student.Username.ToLower().Contains(normalizedKeyword) ||
                e.Student.Email.ToLower().Contains(normalizedKeyword))
            .ToList();
    }

    private static List<ClassEnrollment> ApplyStatusFilter(
        List<ClassEnrollment> enrollments,
        string? status)
    {
        var normalizedStatus = NormalizeStatus(status);

        return normalizedStatus switch
        {
            "active" => enrollments.Where(e => e.Student.IsActive).ToList(),
            "inactive" => enrollments.Where(e => !e.Student.IsActive).ToList(),
            _ => enrollments
        };
    }

    private static List<ClassEnrollment> ApplySorting(
        List<ClassEnrollment> enrollments,
        string? sortBy)
    {
        var normalizedSortBy = NormalizeSortBy(sortBy);

        return normalizedSortBy switch
        {
            "email" => enrollments.OrderBy(e => e.Student.Email).ToList(),
            "date" => enrollments.OrderByDescending(e => e.EnrolledAt).ToList(),
            "status" => enrollments.OrderByDescending(e => e.Student.IsActive).ToList(),
            _ => enrollments.OrderBy(e => e.Student.Username).ToList()
        };
    }

    private static List<ClassEnrollment> ApplyPagination(
        List<ClassEnrollment> enrollments,
        int page,
        int pageSize)
    {
        return enrollments
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    private static ManageStudentListViewModel BuildViewModel(
        Class classEntity,
        List<ClassEnrollment> pagedEnrollments,
        string? keyword,
        string? status,
        string? sortBy,
        int page,
        int pageSize,
        int totalItems,
        int totalPages,
        int totalStudents,
        int activeStudents,
        int inactiveStudents)
    {
        return new ManageStudentListViewModel
        {
            ClassId = classEntity.ClassId,
            ClassName = classEntity.ClassName,

            Keyword = keyword ?? string.Empty,
            Status = NormalizeStatus(status),
            SortBy = NormalizeSortBy(sortBy),

            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,

            TotalStudents = totalStudents,
            ActiveStudents = activeStudents,
            InactiveStudents = inactiveStudents,

            Students = pagedEnrollments.Select(e => new ManageStudentItemViewModel
            {
                StudentId = e.StudentId,
                StudentName = e.Student.Username,
                Email = e.Student.Email,
                IsActive = e.Student.IsActive,
                EnrollmentStatus = e.Student.IsActive ? "Đang hoạt động" : "Không hoạt động",
                EnrolledAt = e.EnrolledAt
            }).ToList()
        };
    }

    private static int NormalizePage(int page)
    {
        return page < 1 ? 1 : page;
    }

    private static int CalculateTotalPages(int totalItems, int pageSize)
    {
        return (int)Math.Ceiling(totalItems / (double)pageSize);
    }

    private static string NormalizeStatus(string? status)
    {
        return string.IsNullOrWhiteSpace(status)
            ? "all"
            : status.Trim().ToLower();
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        return string.IsNullOrWhiteSpace(sortBy)
            ? "name"
            : sortBy.Trim().ToLower();
    }
}