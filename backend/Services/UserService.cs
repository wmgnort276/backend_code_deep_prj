using backend.Data;
using backend.Repository;
using backend.ResponseModel;
using Microsoft.EntityFrameworkCore;

namespace backend.Services
{
    public class UserService : IUserRepository
    {
        private readonly MyDbContext _dbContext;

        public UserService(MyDbContext dbContext) {
            _dbContext = dbContext;
        }
        public List<SubmissionResp> GetUserResolveExercises(string userId)
        {
            var userSubmissions = _dbContext.Submissions
                .Include(item => item.Exercise)
                .Include(item => item.Exercise.ExerciseLevel)
                .Where(item => item.StudentId == userId && item.Status)
                .AsEnumerable()
                .DistinctBy(item => item.ExerciseId)
                .AsQueryable();

            return userSubmissions.Select(item => new SubmissionResp
            {
                StudentId = item.StudentId,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                ExerciseId = item.ExerciseId,
                exerciseName = item.Exercise.Name,
                exerciseLevelName = item.Exercise.ExerciseLevel.Name,
            }).ToList();
        }

        public List<SubmissionResp> GetUserSubmitExercises(string userId)
        {
            var userSubmissions = _dbContext.Submissions
                .Include(item => item.Exercise)
                .Include(item => item.Exercise.ExerciseLevel)
                .Where(item => item.StudentId == userId)
                .AsQueryable();

            return userSubmissions.Select(item => new SubmissionResp
            {
                StudentId = item.StudentId,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                ExerciseId = item.ExerciseId,
                exerciseName = item.Exercise.Name,
                exerciseLevelName = item.Exercise.ExerciseLevel.Name,
            }).ToList();
        }



    }
}
