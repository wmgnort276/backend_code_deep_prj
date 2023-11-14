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
        public List<SubmissionResp> GetUserExercise(string userId)
        {
            var userSubmissions = _dbContext.Submissions
                .Where(item => item.StudentId == userId && item.Status).AsQueryable();

            return userSubmissions.Select(item => new SubmissionResp
            {
                StudentId = item.StudentId,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                ExerciseId = item.ExerciseId,
            }).ToList();  
        }
    }
}
