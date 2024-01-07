using backend.ResponseModel;

namespace backend.Repository
{
    public interface IUserRepository
    {
        public List<SubmissionResp> GetUserResolveExercises(string userId);
        public List<SubmissionResp> GetUserSubmitExercises(string userId);

        public List<UserRank> GetRankingList();

        public int GetUserRanking(string userId);

    }
}
