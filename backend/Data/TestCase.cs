using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    public class TestCase
    {
        public Guid Id { get; set; }

        public Guid ExerciseId { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }

        public int Sequence { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; }
    }
}
