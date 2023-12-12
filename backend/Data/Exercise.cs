using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("Exercise")]
    public class Exercise
    {
        [Key]
        public Guid Id { get; set; }

        public string Name { get; set; }   
        public string Description { get; set; }

        public byte[] RunFile { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ModifiedAt { get; set; }

        public int ExerciseLevelId { get; set; }

        public int ExerciseTypeId { get; set; }

        [ForeignKey("ExerciseLevelId")]
        public ExerciseLevel ExerciseLevel { get; set; }

        [ForeignKey("ExerciseTypeId")]
        public ExerciseType ExerciseType { get; set; }

        [Required]
        public string HintCode { get; set; }

        public int TimeLimit { get; set; }

        public int Score { get; set; }

        public virtual ICollection<Submission> Submissions { get; set; }
    }
}
