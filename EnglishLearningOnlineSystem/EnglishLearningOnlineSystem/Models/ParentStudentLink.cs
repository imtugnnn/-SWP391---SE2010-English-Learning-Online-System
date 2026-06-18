using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EnglishLearningOnlineSystem.Models
{
    public class ParentStudentLink
    {
        [Key]
        public int Id { get; set; }

        public int ParentId { get; set; }
        [ForeignKey(nameof(ParentId))]
        public User? Parent { get; set; }

        public int StudentId { get; set; }
        [ForeignKey(nameof(StudentId))]
        public StudentProfile? Student { get; set; }

        [MaxLength(50)]
        public string? Relationship { get; set; }

        public DateTime LinkedAt { get; set; } = DateTime.UtcNow;
    }
}
