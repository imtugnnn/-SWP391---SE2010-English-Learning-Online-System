using System;
using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.Admin
{
    public class SystemNotificationViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tiêu đề.")]
        [MaxLength(250, ErrorMessage = "Tiêu đề không được vượt quá 250 ký tự.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung.")]
        public string Content { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn đối tượng nhận.")]
        [MaxLength(100)]
        public string Recipient { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại người dùng.")]
        [MaxLength(100)]
        public string UserType { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        [MaxLength(50)]
        public string Status { get; set; }

        public DateTime? PublishTime { get; set; }
    }
}
