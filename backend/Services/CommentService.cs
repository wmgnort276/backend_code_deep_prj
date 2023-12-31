﻿using backend.Data;
using backend.Repository;
using backend.RequestModel;
using backend.ResponseModel;
using Microsoft.EntityFrameworkCore;

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
            var exerciseFound = _dbContext.Exercises.SingleOrDefault(item => item.Id == comment.ExerciseId)
                ?? throw new Exception("Exercise not found!");

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
                .Include(item => item.Users)
                .Where(item => item.ExerciseId == ExerciseId)
                .OrderByDescending(item => item.CreatedAt)
                .AsQueryable();

            return listComments.Select(item => new CommentResp
            {
                UserId = item.UserId,
                Content = item.Content,
                ExerciseId = item.ExerciseId,
                Username = item.Users.UserName,
                Upvote = 0,
                Downvote = 0,
                CreatedAt = item.CreatedAt
            }).ToList();
        }

        public CommentResp UpdateComment(CommentResp comment)
        {
            throw new NotImplementedException();
        }
    }
}
