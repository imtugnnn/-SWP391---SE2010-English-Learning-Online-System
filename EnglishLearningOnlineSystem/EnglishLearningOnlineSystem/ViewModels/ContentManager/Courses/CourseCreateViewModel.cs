using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Courses
{
    public class CourseCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên khoá học")]
        [MaxLength(255)]
        [Display(Name = "Tên khoá học")]
        public string CourseName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn khối lớp")]
        [MaxLength(50)]
        [Display(Name = "Khối lớp")]
        public string GradeLevel { get; set; } = string.Empty;

        [MaxLength(2000)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }


    }
}