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

    // Optional additional domain tables (add if corresponding model classes exist)
    public DbSet<StudentProfile>? StudentProfiles { get; set; }
    public DbSet<Course>? Courses { get; set; }
    public DbSet<Class>? Classes { get; set; }
    public DbSet<Lesson>? Lessons { get; set; }
    public DbSet<WeeklyAssignment>? WeeklyAssignments { get; set; }
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
    public DbSet<TeacherFeedback>? TeacherFeedbacks { get; set; }
    public DbSet<StudentBadge>? StudentBadgeJoin { get; set; } // alternative reference

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
        }
        catch { }

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

        // SEED DATA: roles (preserve previous roles list) and an admin user
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = "Student" },
            new Role { Id = 2, Name = "Admin" },
            new Role { Id = 3, Name = "Teacher" },
            new Role { Id = 4, Name = "Parent" },
            new Role { Id = 5, Name = "Content Manager" }
        );
    }
}
