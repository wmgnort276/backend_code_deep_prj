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
                .Include(item => item.Exercise.ExerciseType)
                .Where(item => item.StudentId == userId && item.Status)
                .AsEnumerable()
                .DistinctBy(item => item.ExerciseId)
                .AsQueryable();

            return userSubmissions.Select(item => new SubmissionResp
            {
                Id = item.Id,
                StudentId = item.StudentId,
                Status = item.Status,
                CreatedAt = item.CreatedAt,
                ExerciseId = item.ExerciseId,
                exerciseName = item.Exercise.Name,
                exerciseLevelName = item.Exercise.ExerciseLevel.Name,
                exerciseTypeName = item.Exercise.ExerciseType.Name,
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

        public List<UserRank> GetRankingList()
        {
            var listRankings = _dbContext.Users
                .Where(u => !_dbContext.UserRoles
                    .Join(_dbContext.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, r.Name })
                     .Any(j => j.UserId == u.Id && j.Name == "admin"))
                .ToList();

            return listRankings.Select(item => new UserRank
            {
                Score = item.Score,
                UserName = item.UserName,
            })
                .OrderByDescending(item => item.Score)
                .Take(10)
                .ToList();
        }

        public int GetUserRanking(string userId)
        {
            var listRankings = _dbContext.Users
                 .Where(u => !_dbContext.UserRoles
                     .Join(_dbContext.Roles,
                         ur => ur.RoleId,
                         r => r.Id,
                         (ur, r) => new { ur.UserId, r.Name })
                      .Any(j => j.UserId == u.Id && j.Name == "admin"))
                 .OrderByDescending(item => item.Score)
                 .ToList();

            var userInfo = listRankings.FindIndex(item => item.Id == userId);
            if(userInfo != -1)
            {
                return userInfo + 1;
            }
            
            return -1;
        }

    }
}
