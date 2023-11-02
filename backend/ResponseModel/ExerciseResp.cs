namespace backend.ResponseModel
{
    public class ExerciseResp
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public byte[] RunFile { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ModifiedAt { get; set; }

        public int ExerciseLevelId { get; set; }

        public int ExerciseTypeId { get; set; }

        public string ExerciseLevelName { get; set; }

        public string ExerciseTypeName { get; set; }
    }
}
