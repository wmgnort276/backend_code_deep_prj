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

            // Find if user has already rating current exercise
            var ratingFound = _dbContext.Rating.SingleOrDefault(item => item.UserId == userId 
                                                            && item.ExerciseId == exerciseFound.Id);
            if (ratingFound != null)
            {
                ratingFound.RatingValue = rating.RatingValue;
                ratingFound.UpdatedAt = new DateTime();
                _dbContext.SaveChanges();
                return new RatingResp
                {
                    RatingValue = rating.RatingValue,
                    ExerciseId = exerciseFound.Id,
                };
            } else
            {
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
        }

        public RatingResp UpdateRating(string userId, RatingModel rating)
        {
            throw new NotImplementedException();
        }
    }
}
