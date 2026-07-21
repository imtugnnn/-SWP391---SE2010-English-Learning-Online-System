//Create by TungDPL
//Create at 6/26/2026
//Last update: 7/21/2026
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class SystemNotification
    {
        [Key]
        public int Id { get; set; }

        // BR-NOTI-01: The notification title and content are mandatory before a notification can be saved or published.
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }

        // BR-NOTI-01: The notification title and content are mandatory before a notification can be saved or published.
        [Required]
        public string Content { get; set; }

        [Required]
        [MaxLength(100)]
        public string Recipient { get; set; } // e.g., "Tất cả người dùng", "Giáo viên", "Học sinh", "Học sinh, Phụ huynh"

        [Required]
        [MaxLength(100)]
        public string UserType { get; set; } // e.g., "Tất cả", "Giáo viên", "Học sinh", "Nhiều vai trò"

        // BR-NOTI-12: The notification status must be one of the predefined values: Draft, Published, or Archived. ("Bản nháp", "Đã phát hành", "Đã hủy", "Đã lên lịch")
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } // "Đã phát hành", "Đã lên lịch", "Bản nháp", "Đã hủy"

        public DateTime? PublishTime { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}
