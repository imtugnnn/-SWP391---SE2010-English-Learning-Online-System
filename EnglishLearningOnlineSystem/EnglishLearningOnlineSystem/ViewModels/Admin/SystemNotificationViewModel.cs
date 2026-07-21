using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Admin
{
    public class SystemNotificationViewModel
    {
        public int Id { get; set; }

        // BR-NOTI-01: The notification title and content are mandatory before a notification can be saved or published.
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [MaxLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự.")]
        public string Title { get; set; }

        // BR-NOTI-01: The notification title and content are mandatory before a notification can be saved or published.
        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đối tượng nhận.")]
        [MaxLength(100)]
        public string Recipient { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại người dùng.")]
        [MaxLength(100)]
        public string UserType { get; set; }

        // BR-NOTI-12: The notification status must be one of the predefined values: Draft, Published, or Archived. ("Bản nháp", "Đã phát hành", "Đã hủy", "Đã lên lịch")
        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime? PublishTime { get; set; }
    }
}
