using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class Vocabulary
    {
        [Key]
        public int VocabularyId { get; set; }

        [Required, MaxLength(255)]
        public string Word { get; set; }

        [Required]
        public string Meaning { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        public string? ExampleSentence { get; set; }

        [MaxLength(500)]
        public string? AudioUrl { get; set; }

        public int LessonId { get; set; }
        [ForeignKey("LessonId")]
        public Lesson Lesson { get; set; }
    }
}
