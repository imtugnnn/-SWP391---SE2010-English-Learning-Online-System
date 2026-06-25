using System.ComponentModel.DataAnnotations;

namespace EnglishLearningOnlineSystem.ViewModels.ContentManager.Quizzes
{
    public class QuizEditViewModel
    {
        public int QuizId { get; set; }

        [Required(ErrorMessage = "Câu hỏi là bắt buộc")]
        [Display(Name = "Câu hỏi")]
        public string Question { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại câu hỏi là bắt buộc")]
        [Display(Name = "Loại câu hỏi")]
        public string QuizType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Các lựa chọn là bắt buộc")]
        [Display(Name = "Các lựa chọn (Định dạng JSON)")]
        public string Options { get; set; } = string.Empty;

        [Required(ErrorMessage = "Câu trả lời đúng là bắt buộc")]
        [Display(Name = "Câu trả lời đúng")]
        public string CorrectAnswer { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn bài học")]
        [Display(Name = "Bài học")]
        public int LessonId { get; set; }
    }
}
