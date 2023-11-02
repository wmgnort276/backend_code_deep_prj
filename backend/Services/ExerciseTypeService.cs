using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Services
{
    public class ExerciseTypeService : IExerciseTypeRepository
    {
        private readonly MyDbContext _dbContext;

        public ExerciseTypeService(MyDbContext dbContext) {
            _dbContext = dbContext;
        }

        public ExerciseTypeResp Add(ExerciseTypeModel exerciseType)
        {
            var existExerciseType = _dbContext.ExerciseTypes.SingleOrDefault(item => item.Name == exerciseType.Name);
            if (existExerciseType != null)
            {
                throw new InvalidOperationException("Exercise type exist!");
            }

            var newExerciseType = new ExerciseType
            {
                Name = exerciseType.Name,
            };

            _dbContext.Add(newExerciseType);
            _dbContext.SaveChanges();
            return new ExerciseTypeResp
            {
                Name = newExerciseType.Name,
                Id = newExerciseType.Id
            };

        }

        public List<ExerciseTypeResp> GetAll()
        {
            var exerciseTypes = _dbContext.ExerciseTypes.Select(item => new ExerciseTypeResp
            {
                Id = item.Id,
                Name = item.Name,
            }).ToList();

            return exerciseTypes;
        }

        public ExerciseTypeResp GetById(int id)
        {
             throw new NotImplementedException();
        }
    }
}
