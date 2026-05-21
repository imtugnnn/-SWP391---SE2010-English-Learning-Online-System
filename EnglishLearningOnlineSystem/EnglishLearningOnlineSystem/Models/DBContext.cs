using Microsoft.EntityFrameworkCore;

namespace EnglishLearningOnlineSystem.Models;

public class DBContext : DbContext
{
    public DBContext(DbContextOptions<DBContext> options) : base(options)
{
}

// Các bảng sẽ xuất hiện trong SQL Server [cite: 34]
public DbSet<User> Users { get; set; }
// public DbSet<Course> Courses { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Tạo sẵn dữ liệu mẫu để khi DB sinh ra là có sẵn data luôn [cite: 34]
    modelBuilder.Entity<User>().HasData(
        new User { Id = 1, Username = "admin", Email = "admin@english.com", Password = BCrypt.Net.BCrypt.HashPassword("123") }
    );
}
}