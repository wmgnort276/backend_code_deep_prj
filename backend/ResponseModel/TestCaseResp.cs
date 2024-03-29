﻿namespace backend.ResponseModel
{
    public class TestCaseResp
    {
        public Guid? Id { get; set; }

        public Guid? ExerciseId { get; set; }

        public string Input { get; set; }

        public string Output { get; set; }

        public string Expected {  get; set; }

        public bool Status { get; set; }
    }
}
