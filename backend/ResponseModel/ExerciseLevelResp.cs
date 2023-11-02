using System.ComponentModel.DataAnnotations;

namespace backend.ResponseModel
{
    public class ExerciseLevelResp
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int Score { get; set; }
    }
}
