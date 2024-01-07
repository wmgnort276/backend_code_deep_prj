namespace backend.RequestModel
{
    public class TestCaseModel
    {
        public Guid Id { get; set; }

        public Guid ExerciseId { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }
    }
}
