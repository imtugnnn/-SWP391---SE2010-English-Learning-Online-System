using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class FlashcardCardResult
    {
        [Key]
        public int ResultId { get; set; }

        public bool KnewIt { get; set; } 

        // Foreign keys
        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public FlashcardSession Session { get; set; }

        public int VocabularyId { get; set; }
        [ForeignKey("VocabularyId")]
        public Vocabulary Vocabulary { get; set; }
    }
}
