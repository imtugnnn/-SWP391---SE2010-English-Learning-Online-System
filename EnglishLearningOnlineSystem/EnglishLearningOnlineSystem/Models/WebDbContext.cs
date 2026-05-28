using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Models;

public class WebDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public WebDbContext(DbContextOptions<WebDbContext> options) : base(options)
{
}

// Các bảng sẽ xuất hiện trong SQL Server [cite: 34]
public DbSet<User> Users { get; set; }
public DbSet<Role> Roles { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    modelBuilder.Entity<Role>().HasData(
        new Role { Id = 1, Name = "Student" },
        new Role { Id = 2, Name = "Admin"},
        new Role { Id = 3, Name = "Teacher" },
        new Role { Id = 4, Name = "Parent" },
        new Role { Id = 5, Name = "Content Manager" }
    );

    modelBuilder.Entity<User>().HasData(
        new User
        {
            Id = 1,
            Username = "admin",
            Email = "admin@english.com",
            Password = "$2a$11$tY9pM.ggrCz7ECFXds5OCex1iv6.MBN/UV6klKRUj/KfdKUCuwMGO",
            IsActive = true,
            RoleId = 2
        }
    );
}
}
