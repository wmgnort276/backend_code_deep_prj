using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Repository
{
    public interface ISubmissionRepository
    {
        SubmissionResp Add(SubmissionModel model);

        List<SubmissionResp> GetUserSubmission(string userId, Guid exerciseId);

        SubmissionResp GetSubmissionById(Guid id);
    }
}
