using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Services
{
    public class SubmissionService : ISubmissionRepository
    {
        private readonly MyDbContext _dbContext;

        public SubmissionService(MyDbContext dbContext) { 
            _dbContext = dbContext;
        }
        public SubmissionResp Add(SubmissionModel model)
        {
            var newSubmission = new Submission
            {
                Status = model.Status,
                StudentId = model.StudentId,    
                ExerciseId = model.ExerciseId,
            };

            _dbContext.Add(newSubmission);
            _dbContext.SaveChanges();

            return new SubmissionResp 
            { 
                Status = newSubmission.Status,
                StudentId = newSubmission.StudentId,
                ExerciseId = newSubmission.ExerciseId
            };
        }

        public List<SubmissionResp> GetUserSubmission(string userId, Guid exerciseId)
        {
            var submissions = _dbContext.Submissions
                .Where(item => item.StudentId == userId && item.ExerciseId == exerciseId)
                .OrderByDescending(item => item.CreatedAt)
                .AsQueryable();

            return submissions.Select(item => new SubmissionResp
            {
                Id = item.Id,
                StudentId = item.StudentId,
                ExerciseId = item.ExerciseId,
                CreatedAt = item.CreatedAt,
                Status = item.Status,
                Runtime = item.Runtime,
                Memory = item.Memory,
            }).ToList();
            
            throw new NotImplementedException();
        }
    }
}
