using backend.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.RequestModel
{
    public class ExerciseModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public IFormFile File { get; set; }

        public int ExerciseLevelId { get; set; }

        public int ExerciseTypeId { get; set; }
    }
}
