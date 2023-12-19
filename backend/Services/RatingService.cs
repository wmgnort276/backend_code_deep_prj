using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Services
{
    public class RatingService : IRatingRepository
    {
        private readonly MyDbContext _dbContext;

        public RatingService(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public RatingResp CreateRating(string userId, RatingModel rating)
        {
            // Find if exercise exist
            var exerciseFound = _dbContext.Exercises.SingleOrDefault(item => item.Id == rating.ExerciseId) 
                ?? throw new Exception("Exercise not found!");
            var ratingModel = new Rating
            {
                UserId = userId,
                CreatedAt = new DateTime(),
                RatingValue = rating.RatingValue,
                ExerciseId = rating.ExerciseId,
            };

            _dbContext.Add(ratingModel);
            _dbContext.SaveChanges();
            return new RatingResp
            {
                RatingValue = ratingModel.RatingValue,
                ExerciseId = ratingModel.ExerciseId,
            };
        }

        public RatingResp UpdateRating(string userId, RatingModel rating)
        {
            throw new NotImplementedException();
        }
    }
}
