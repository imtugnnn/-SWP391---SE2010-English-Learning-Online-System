using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.Models
{
    public class SystemNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        [MaxLength(100)]
        public string Recipient { get; set; } // e.g., "Tất cả người dùng", "Giáo viên", "Học sinh", "Học sinh, Phụ huynh"

        [Required]
        [MaxLength(100)]
        public string UserType { get; set; } // e.g., "Tất cả", "Giáo viên", "Học sinh", "Nhiều vai trò"

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // "Đã phát hành", "Đã lên lịch", "Bản nháp", "Đã hủy"

        public DateTime? PublishTime { get; set; }

        [MaxLength(100)]
        public string Creator { get; set; } = "Administrator";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
