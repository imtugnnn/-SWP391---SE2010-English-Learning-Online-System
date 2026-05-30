using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        public string Question { get; set; }

        [MaxLength(100)]
        public string QuizType { get; set; }
        public string Options { get; set; } // Lưu chuỗi định dạng JSON câu trả lời

        [Required]
        public string CorrectAnswer { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }
    }
}
