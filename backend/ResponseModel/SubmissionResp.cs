namespace backend.ResponseModel
{
    public class SubmissionResp
    {
        public Guid Id { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StudentId { get; set; }

        public Guid ExerciseId { get; set; }

        public string exerciseName { get; set; }

        public string exerciseLevelName { get; set;}

        public int Memory { get; set; }

        public int Runtime { get; set; }
    }
}
