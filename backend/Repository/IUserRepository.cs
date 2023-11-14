using backend.ResponseModel;

namespace backend.Repository
{
    public interface IUserRepository
    {
        public List<SubmissionResp> GetUserExercise(string userId);
    }
}
