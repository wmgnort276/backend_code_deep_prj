using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Services
{
    public class CommentService : ICommentRepository
    {
        private readonly MyDbContext _dbContext;

        public CommentService(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public CommentResp CreateComment(CommentModel comment, string userId)
        {
            var newComment = new Comment
            {
                UserId = userId,
                Content = comment.Content,
                ExerciseId = comment.ExerciseId,
                Upvote = 0,
                Downvote = 0,
            };

            _dbContext.Add(newComment);
            _dbContext.SaveChanges();

            return new CommentResp
            {
                UserId = newComment.UserId,
                Content = newComment.Content,
                ExerciseId = newComment.ExerciseId,
                Upvote = 0,
                Downvote = 0,
            };
        }

        public bool DeleteComment(Guid Id)
        {
            throw new NotImplementedException();
        }

        public List<CommentResp> GetComments(Guid ExerciseId)
        {
            var listComments = _dbContext.Comments
                .Where(item => item.ExerciseId == ExerciseId)
                .OrderByDescending(item => item.CreatedAt)
                .AsQueryable();

            return listComments.Select(item => new CommentResp
            {
                UserId = item.UserId,
                Content = item.Content,
                ExerciseId = item.ExerciseId,
                Upvote = 0,
                Downvote = 0,
            }).ToList();
            throw new NotImplementedException();
        }

        public CommentResp UpdateComment(CommentResp comment)
        {
            throw new NotImplementedException();
        }
    }
}
