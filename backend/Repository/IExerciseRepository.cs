using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Repository
{
    public interface IExerciseRepository
    {
        List<ExerciseResp> All(int pageIndex = 1, int pageSize = 5);

        ExerciseResp? GetById(Guid id, string userId);

        ExerciseResp Add(ExerciseModel model,byte[] file);

        byte[]? DownLoadrunFille(Guid id);

        string Submit(Guid id, string userId, SourceCode example);
    }
}
