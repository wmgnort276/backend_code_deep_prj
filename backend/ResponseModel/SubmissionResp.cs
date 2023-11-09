namespace backend.ResponseModel
{
    public class SubmissionResp
    {
        public Guid Id { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StudentId { get; set; }

        public Guid ExerciseId { get; set; }
    }
}
