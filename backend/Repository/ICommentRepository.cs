using backend.RequestModel;
using backend.ResponseModel;

namespace backend.Repository
{
    public interface ICommentRepository
    {
        List<CommentResp> GetComments(Guid ExerciseId);

        CommentResp CreateComment(CommentModel commet, string userId);

        CommentResp UpdateComment(CommentResp comment);

        bool DeleteComment(Guid Id);
    }
}
