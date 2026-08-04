using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels;

// ViewModel dùng để hiển thị và cập nhật hồ sơ học sinh
public class StudentProfileViewModel
{
    // Thông tin tài khoản
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public DateTime? BirthDate { get; set; }
    public string FullName { get; set; } = "";

    // Thông tin hồ sơ
    public string Nickname { get; set; } = "";
    public string AvatarUrl { get; set; } = "";

    // Thống kê học tập
    public int Level { get; set; }
    public int XP { get; set; }
    public int CurrentStreakDays { get; set; }

    // Dữ liệu từ form cập nhật
    public string NewNickname { get; set; } = "";
    [Required, StringLength(150, MinimumLength = 2)]
    public string NewFullName { get; set; } = "";
    public IFormFile? AvatarFile { get; set; }

    // Thông báo kết quả thao tác
    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }
}
