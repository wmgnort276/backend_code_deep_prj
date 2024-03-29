﻿using backend.Data;

namespace backend.ResponseModel
{
    public class ExerciseResponseAdmin
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public byte[]? RunFile { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime ModifiedAt { get; set; }

        public int ExerciseLevelId { get; set; }

        public int ExerciseTypeId { get; set; }

        public string ExerciseLevelName { get; set; }

        public string ExerciseTypeName { get; set; }


        public float TimeLimit { get; set; }

        public int Score { get; set; }

        public double Rating { get; set; }

        public int RatingCount { get; set; }

        public virtual ICollection<TestCase>? TestCases { get; set; }

        public int SubmittedNumber { get; set; }

        public float RatingValue { get; set; }

        public bool IsPublic { get; set; }
    }
}
