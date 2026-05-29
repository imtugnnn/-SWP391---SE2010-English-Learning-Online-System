using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class QuizAttemptAnswer
    {
        [Key]
        public int AnswerId { get; set; }

        [Required]
        public string SelectedAnswer { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

        public int AttemptId { get; set; }
        [ForeignKey("AttemptId")]
        public QuizAttempt Attempt { get; set; }

        public int QuizId { get; set; } // References the flat Quiz model (each row = a question)
        [ForeignKey("QuizId")]
        public Quiz Quiz { get; set; }
    }
}
