namespace backend.ResponseModel
{
    public class RatingResp
    {
        public Guid ExerciseId { get; set; }

        public int RatingValue { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}
