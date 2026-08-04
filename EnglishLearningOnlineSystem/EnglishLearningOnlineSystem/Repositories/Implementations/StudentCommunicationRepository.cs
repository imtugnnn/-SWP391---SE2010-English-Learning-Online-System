using EnglishLearningOnlineSystem.Data;
using EnglishLearningOnlineSystem.Models;
using EnglishLearningOnlineSystem.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Repositories.Implementations;

public class StudentCommunicationRepository : IStudentCommunicationRepository
{
    private readonly AppDbContext _db;

    public StudentCommunicationRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Notification>> GetNotificationsAsync(int studentId)
    {
        return _db.Notifications!
            .AsNoTracking()
            .Include(x => x.Assignment)
                .ThenInclude(x => x!.Lesson)
            .Where(x => x.UserId == studentId)
            .OrderByDescending(x => x.CreateAt)
            .ToListAsync();
    }

    public Task<Notification?> GetNotificationAsync(int notificationId, int studentId)
    {
        // Ownership nằm trong câu query để Student không thể sửa notification của tài khoản khác.
        return _db.Notifications!.FirstOrDefaultAsync(x =>
            x.NotificationId == notificationId && x.UserId == studentId);
    }

    public Task<List<TeacherFeedback>> GetFeedbacksAsync(int studentId)
    {
        return _db.TeacherFeedbacks!
            .AsNoTracking()
            .Include(x => x.Teacher)
            .Include(x => x.Class)
            .Include(x => x.Assignment)
                .ThenInclude(x => x!.Lesson)
            .Where(x => x.StudentId == studentId)
            .OrderByDescending(x => x.CreateAt)
            .ToListAsync();
    }

    public Task<TeacherFeedback?> GetFeedbackAsync(int feedbackId, int studentId)
    {
        return _db.TeacherFeedbacks!.FirstOrDefaultAsync(x =>
            x.FeedbackId == feedbackId && x.StudentId == studentId);
    }

    public Task<List<Notification>> GetUnreadNotificationsAsync(int studentId, int? feedbackId = null)
    {
        return _db.Notifications!
            .Where(x => x.UserId == studentId && !x.IsRead &&
                        (!feedbackId.HasValue || x.FeedbackId == feedbackId.Value))
            .ToListAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
