using backend.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.RequestModel
{
    public class SubmissionModel
    {
        public Guid Id { get; set; }

        public bool Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public string StudentId { get; set; }

        public Guid ExerciseId { get; set; }

        public int Memory { get; set; }

        public int Runtime { get; set; }
    }
}
