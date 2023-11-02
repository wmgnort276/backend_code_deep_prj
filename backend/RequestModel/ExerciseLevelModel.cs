using System.ComponentModel.DataAnnotations;

namespace backend.RequestModel
{
    public class ExerciseLevelModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public int Score { get; set; }
    }
}
