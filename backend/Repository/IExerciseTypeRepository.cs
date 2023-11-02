using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Repository
{
    public interface IExerciseTypeRepository
    {
        List<ExerciseTypeResp> GetAll();

        ExerciseTypeResp? GetById(int id);

        ExerciseTypeResp Add(ExerciseTypeModel exerciseType);
    }
}
