namespace backend.ResponseModel
{
    public class UserStatisticResp
    {
        public int TotalExerciseCount { get; set; }
        public int ExerciseResolvedCount { get; set; }

        public int TotalEasyExerciseCount { get; set; }

        public int ExerciseEasyResolvedCount { get; set; }

        public int TotalMediumExerciseCount { get; set; }

        public int ExerciseMediumResolvedCount { get; set; }

        public int TotalHardExerciseCount { get; set; }

        public int ExerciseHardResolvedCount { get; set; }

    }
}
