namespace backend.ResponseModel
{
    public class CommentResp
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public Guid ExerciseId { get; set; }

        public string Content { get; set; }

        public int Upvote { get; set; }

        public int Downvote { get; set; }
    }
}
