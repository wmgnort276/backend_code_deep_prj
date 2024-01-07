using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("Submission")]
    public class Submission
    {
        [Key]
        public Guid Id { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }

        [ForeignKey("StudentId")]
        public Users Users { get; set; }

        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; }

        public string StudentId { get; set; }

        public Guid ExerciseId { get; set; }

        public int Memory { get; set; }

        public int Runtime { get; set; }

        public string? SourceCode { get; set; }
    }
}
