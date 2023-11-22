namespace backend.RequestModel
{
    public class CommentModel
    {
        public string UserId { get; set; }

        public string Content { get; set; }

        public Guid ExerciseId { get; set; }

        public int Upvote { get; set; }

        public int Downvote { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
