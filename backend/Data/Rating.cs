using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("Rating")]
    public class Rating
    {

        public Guid Id { get; set; }
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Users User { get; set; }

        public Guid ExerciseId { get; set; }

        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; }

        public int RatingValue { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
