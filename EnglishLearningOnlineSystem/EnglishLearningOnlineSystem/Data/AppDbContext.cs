using Microsoft.EntityFrameworkCore;
using EnglishLearningOnlineSystem.Models;

namespace EnglishLearningOnlineSystem.Data;

public class AppDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Core user & role tables (existing)
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    // Optional additional domain tables (add if corresponding model classes exist)
    public DbSet<AcademicYear>? AcademicYears { get; set; }
    public DbSet<StudentProfile>? StudentProfiles { get; set; }
    public DbSet<Course>? Courses { get; set; }
    public DbSet<Class>? Classes { get; set; }
    public DbSet<ClassEnrollment>? ClassEnrollments { get; set; }
    public DbSet<Lesson>? Lessons { get; set; }
    public DbSet<WeeklyAssignment>? WeeklyAssignments { get; set; }
    public DbSet<WeeklyAssignmentVocabulary> WeeklyAssignmentVocabularies { get; set; }
    public DbSet<WeeklyAssignmentQuiz> WeeklyAssignmentQuizzes { get; set; }
    public DbSet<WeeklyAssignmentMiniGame> WeeklyAssignmentMiniGames { get; set; }
    public DbSet<MiniGame>? MiniGames { get; set; }
    public DbSet<Quiz>? Quizzes { get; set; }
    public DbSet<Vocabulary>? Vocabularies { get; set; }
    public DbSet<Progress>? Progresses { get; set; }
    public DbSet<StudentGameProgress>? StudentGameProgresses { get; set; }
    public DbSet<XpTransaction>? XpTransactions { get; set; }
    public DbSet<DailyMission>? DailyMissions { get; set; }
    public DbSet<StudentMission>? StudentMissions { get; set; }
    public DbSet<Badge>? Badges { get; set; }
    public DbSet<StudentBadge>? StudentBadges { get; set; }
    public DbSet<Notification>? Notifications { get; set; }
    public DbSet<SystemNotification>? SystemNotifications { get; set; }
    public DbSet<TeacherFeedback>? TeacherFeedbacks { get; set; }
    public DbSet<StudentBadge>? StudentBadgeJoin { get; set; } // alternative reference

    // Quiz attempt tracking & Flashcard sessions
    public DbSet<QuizAttempt> QuizAttempts { get; set; }
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; set; }
    public DbSet<FlashcardSession> FlashcardSessions { get; set; }
    public DbSet<FlashcardCardResult> FlashcardCardResults { get; set; }
    public DbSet<AssignmentProgress> AssignmentProgresses { get; set; }
    public DbSet<AssignmentActivityProgress> AssignmentActivityProgresses { get; set; }
    public DbSet<SystemSetting> SystemSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure many-to-many StudentBadge if entities exist
        // (This will be ignored if StudentBadge/Student/Badge types are not present)
        try
        {
            modelBuilder.Entity<StudentBadge>(eb =>
            {
                eb.HasKey(sb => new { sb.StudentId, sb.BadgeId });

                eb.HasOne(sb => sb.Student)
                  .WithMany(s => s.StudentBadges)
                  .HasForeignKey(sb => sb.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(sb => sb.Badge)
                  .WithMany(b => b.StudentBadges)
                  .HasForeignKey(sb => sb.BadgeId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
        }
        catch
        {
            // If types are not present, ignore fluent configuration errors at startup compile-time.
        }

        try
        {
            modelBuilder.Entity<AcademicYear>(eb =>
            {
                eb.HasIndex(y => y.YearLabel).IsUnique();
            });
        }
        catch { }

        // Prevent accidental cascade-delete cycles for known relationships if those entities exist.
        // 1. Role - User: deleting a Role should not delete Users.
        try
        {
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        catch { }

        // 2. Class -> Teacher: when deleting a teacher, set TeacherId to null on Class.
        try
        {
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Teacher)
                .WithMany(u => u.TaughtClasses)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        }
        catch { }

        try
        {
            modelBuilder.Entity<Class>()
                .HasOne(c => c.AcademicYear)
                .WithMany(y => y.Classes)
                .HasForeignKey(c => c.AcademicYearId)
                .OnDelete(DeleteBehavior.Cascade);
        }
        catch { }

        try
        {
            modelBuilder.Entity<ClassEnrollment>(eb =>
            {
                eb.HasIndex(x => new { x.ClassId, x.StudentId }).IsUnique();

                eb.HasOne(x => x.Class)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(x => x.ClassId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(x => x.Student)
                  .WithMany(u => u.ClassEnrollments)
                  .HasForeignKey(x => x.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
        }
        catch { }

        try
        {
            modelBuilder.Entity<PasswordResetToken>(eb =>
            {
                eb.HasIndex(t => t.Token).IsUnique();
                eb.HasIndex(t => t.UserId);

                eb.HasOne(t => t.User)
                  .WithMany()
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            });
        }
        catch { }

        // 3. TeacherFeedback rules
        try
        {
            modelBuilder.Entity<TeacherFeedback>()
                .HasOne(f => f.Teacher)
                .WithMany(u => u.GivenFeedbacks)
                .HasForeignKey(f => f.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TeacherFeedback>()
                .HasOne(f => f.Student)
                .WithMany(s => s.ReceivedFeedbacks)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TeacherFeedback>()
                .HasOne(f => f.Class)
                .WithMany()
                .HasForeignKey(f => f.ClassId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TeacherFeedback>()
                .HasOne(f => f.Assignment)
                .WithMany()
                .HasForeignKey(f => f.AssignmentId)
                .OnDelete(DeleteBehavior.SetNull);
        }
        catch { }

        modelBuilder.Entity<Notification>(eb =>
        {
            eb.HasOne(x => x.Assignment)
              .WithMany()
              .HasForeignKey(x => x.AssignmentId)
              .OnDelete(DeleteBehavior.SetNull);
            eb.HasOne(x => x.Feedback)
              .WithMany()
              .HasForeignKey(x => x.FeedbackId)
              .OnDelete(DeleteBehavior.SetNull);
            eb.HasIndex(x => new { x.UserId, x.IsRead, x.CreateAt });
        });

        // 4. WeeklyAssignment -> Course : Restrict
        try
        {
            modelBuilder.Entity<WeeklyAssignment>()
                .HasOne(wa => wa.Course)
                .WithMany(c => c.WeeklyAssignments)
                .HasForeignKey(wa => wa.CourseId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        catch { }

        modelBuilder.Entity<WeeklyAssignment>(eb =>
        {
            eb.HasOne(x => x.Class)
              .WithMany()
              .HasForeignKey(x => x.ClassId)
              .OnDelete(DeleteBehavior.Restrict);

            eb.HasIndex(x => new { x.ClassId, x.LessonId, x.WeekStartDate });
        });

        modelBuilder.Entity<AssignmentProgress>(eb =>
        {
            eb.HasIndex(x => new { x.AssignmentId, x.StudentId }).IsUnique();
            eb.HasOne(x => x.Assignment)
              .WithMany(x => x.StudentProgresses)
              .HasForeignKey(x => x.AssignmentId)
              .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(x => x.Student)
              .WithMany(x => x.AssignmentProgresses)
              .HasForeignKey(x => x.StudentId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssignmentActivityProgress>(eb =>
        {
            eb.HasIndex(x => new { x.AssignmentId, x.StudentId, x.ActivityType, x.ActivityId })
              .IsUnique();
            eb.HasOne(x => x.Assignment)
              .WithMany(x => x.ActivityProgresses)
              .HasForeignKey(x => x.AssignmentId)
              .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(x => x.Student)
              .WithMany(x => x.AssignmentActivityProgresses)
              .HasForeignKey(x => x.StudentId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WeeklyAssignmentVocabulary>(eb =>
        {
            eb.HasKey(x => new { x.AssignmentId, x.VocabularyId });
            eb.HasOne(x => x.Assignment)
              .WithMany(x => x.Vocabularies)
              .HasForeignKey(x => x.AssignmentId)
              .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(x => x.Vocabulary)
              .WithMany()
              .HasForeignKey(x => x.VocabularyId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WeeklyAssignmentQuiz>(eb =>
        {
            eb.HasKey(x => new { x.AssignmentId, x.QuizId });
            eb.HasOne(x => x.Assignment)
              .WithMany(x => x.Quizzes)
              .HasForeignKey(x => x.AssignmentId)
              .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(x => x.Quiz)
              .WithMany()
              .HasForeignKey(x => x.QuizId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<WeeklyAssignmentMiniGame>(eb =>
        {
            eb.HasKey(x => new { x.AssignmentId, x.GameId });
            eb.HasOne(x => x.Assignment)
              .WithMany(x => x.MiniGames)
              .HasForeignKey(x => x.AssignmentId)
              .OnDelete(DeleteBehavior.Cascade);
            eb.HasOne(x => x.MiniGame)
              .WithMany()
              .HasForeignKey(x => x.GameId)
              .OnDelete(DeleteBehavior.Restrict);
        });

        try
        {
            modelBuilder.Entity<QuizAttempt>(eb =>
            {
                eb.HasOne(qa => qa.Student)
                  .WithMany(sp => sp.QuizAttempts)
                  .HasForeignKey(qa => qa.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

                eb.HasOne(qa => qa.Lesson)
                  .WithMany()
                  .HasForeignKey(qa => qa.LessonId)
                  .OnDelete(DeleteBehavior.Restrict);

                eb.HasOne(qa => qa.WeeklyAssignment)
                  .WithMany()
                  .HasForeignKey(qa => qa.WeeklyAssignmentId)
                  .OnDelete(DeleteBehavior.SetNull);

                eb.HasIndex(qa => new { qa.StudentId, qa.SubmittedAt });
            });

            modelBuilder.Entity<QuizAttemptAnswer>(eb =>
            {
                eb.HasOne(a => a.Attempt)
                  .WithMany(qa => qa.Answers)
                  .HasForeignKey(a => a.AttemptId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(a => a.Quiz)
                  .WithMany()
                  .HasForeignKey(a => a.QuizId)
                  .OnDelete(DeleteBehavior.Restrict);

                eb.HasIndex(a => a.AttemptId);
            });
        }
        catch { }

        try
        {
            modelBuilder.Entity<FlashcardSession>(eb =>
            {
                eb.HasOne(fs => fs.Student)
                  .WithMany(sp => sp.FlashcardSessions)
                  .HasForeignKey(fs => fs.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);

                eb.HasOne(fs => fs.Lesson)
                  .WithMany()
                  .HasForeignKey(fs => fs.LessonId)
                  .OnDelete(DeleteBehavior.Restrict);

                eb.HasIndex(fs => fs.StudentId);

                eb.HasOne(fs => fs.WeeklyAssignment)
                  .WithMany()
                  .HasForeignKey(fs => fs.WeeklyAssignmentId)
                  .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<FlashcardCardResult>(eb =>
            {
                eb.HasOne(cr => cr.Session)
                  .WithMany(fs => fs.CardResults)
                  .HasForeignKey(cr => cr.SessionId)
                  .OnDelete(DeleteBehavior.Cascade);

                eb.HasOne(cr => cr.Vocabulary)
                  .WithMany()
                  .HasForeignKey(cr => cr.VocabularyId)
                  .OnDelete(DeleteBehavior.Restrict);
            });
        }
        catch { }

        modelBuilder.Entity<StudentGameProgress>(eb =>
        {
            eb.HasOne(x => x.WeeklyAssignment)
              .WithMany()
              .HasForeignKey(x => x.WeeklyAssignmentId)
              .OnDelete(DeleteBehavior.SetNull);
        });

        // SEED DATA: roles (preserve previous roles list) and an admin user
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Student" },
            new Role { Id = 2, Name = "Admin" },
            new Role { Id = 3, Name = "Teacher" },
            new Role { Id = 5, Name = "Content Manager" }
        );
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        UpdateTimestamps();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<User>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreateAt = DateTime.UtcNow;
                entry.Entity.UpdateAt = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdateAt = DateTime.UtcNow;
            }
        }
    }
}
