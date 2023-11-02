using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Data
{
    [Table("ExerciseLevel")]
    public class ExerciseLevel
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }

        [Required]
        public int Score { get; set; }

        public virtual ICollection<Exercise> Exercises { get; set; }
    }
}
