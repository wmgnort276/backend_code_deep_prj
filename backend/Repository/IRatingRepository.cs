using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Repository
{
    public interface IRatingRepository
    {
        RatingResp CreateRating(string userId, RatingModel rating);

        RatingResp UpdateRating(string userId, RatingModel rating);
    }
}
