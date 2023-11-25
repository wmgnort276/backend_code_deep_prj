namespace backend.RequestModel
{
    public class CommentModel
    {
        public string? UserId { get; set; }

        public string Content { get; set; }

        public Guid ExerciseId { get; set; }
    }
}
