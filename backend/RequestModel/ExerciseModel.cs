using backend.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.RequestModel
{
    public class ExerciseModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public IFormFile? File { get; set; }

        public IFormFile? TestFile { get; set; }

        public IFormFile? FileJava { get; set; }

        public IFormFile? TestFileJava { get; set; }

        public int ExerciseLevelId { get; set; }

        public int ExerciseTypeId { get; set; }

        public string HintCode { get; set; }

        public string HintCodeJava { get; set; }

        public int TimeLimit { get; set; }

        public string? Input1 { get; set; }

        public string? Output1 { get; set; }
        public string? Input2 { get; set; }

        public string? Output2 { get; set; }

        public string? Input3 { get; set; }

        public string? Output3 { get; set; }
    }
}
