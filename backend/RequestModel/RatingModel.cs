namespace backend.RequestModel
{
    public class RatingModel
    {
        public Guid ExerciseId { get; set; }

        public int RatingValue { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
