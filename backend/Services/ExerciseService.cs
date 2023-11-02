using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class ExerciseService : IExerciseRepository
    {
        private readonly MyDbContext _dbContext;

        public ExerciseService(MyDbContext dbContext) {
            _dbContext = dbContext;
        }
        public ExerciseResp Add(ExerciseModel model, byte[] file)
        {
          var newExercise = new Exercise
           {
             Name = model.Name,
             Description = model.Description,
             // CreatedAt = DateTime.Now,
             // ModifiedAt = DateTime.Now,
             ExerciseLevelId = model.ExerciseLevelId,
             ExerciseTypeId = model.ExerciseTypeId,
             RunFile = file
          };

            _dbContext.Exercises.Add(newExercise);
            _dbContext.SaveChanges();

            return new ExerciseResp
              {
              Id = newExercise.Id,
              Name = newExercise.Name,
              Description = newExercise.Description,
              CreatedAt = newExercise.CreatedAt,
            };
            // throw new NotImplementedException();
        }

        public List<ExerciseResp> All(int pageIndex = 1, int pageSize = 5)
        {
            var exercises = _dbContext.Exercises.Include(item => item.ExerciseLevel)
                    .Include(item => item.ExerciseType).AsQueryable();

            var result = PaginatedList<Exercise>.Create(exercises, pageIndex, pageSize);
            return result.Select(item => new ExerciseResp
            {
                Id = item.Id,
                Name = item.Name,
                Description = item.Description,
                ExerciseLevelId = item.ExerciseLevelId,
                ExerciseTypeId=item.ExerciseTypeId,
                ExerciseLevelName = item.ExerciseLevel?.Name,
                ExerciseTypeName = item.ExerciseType?.Name,
            }).ToList();

            //   throw new NotImplementedException();
        }

        public ExerciseResp? GetById(Guid id)
        {
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);
            if (exercise != null)
            {
                return new ExerciseResp
                {
                    Id = exercise.Id,
                    Name = exercise.Name,
                    Description = exercise.Description,
                    ExerciseLevelId = exercise.ExerciseLevelId,
                    ExerciseTypeId = exercise.ExerciseTypeId,
                    RunFile = exercise.RunFile
                };
            }

            return null;
            // throw new NotImplementedException();
        }

        public byte[]? DownLoadrunFille(Guid id)
        {
            var exercise = _dbContext.Exercises.SingleOrDefault(item => item.Id == id);

            if (exercise != null)
            {
                return exercise?.RunFile;
            }

            return null;
        }
    }
}
