using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace backend.Data
{
    public class MyDbContext : IdentityDbContext<Users>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        public DbSet<Users> Users { get; set; }
        public DbSet<ExerciseType> ExerciseTypes { get; set; }

        public DbSet<ExerciseLevel> ExerciseLevels { get; set; }

        public DbSet<Exercise> Exercises { get; set; }

        public DbSet<Submission> Submissions { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Rating> Rating { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Exercise>(exercise =>
            {
                exercise.Property(ex => ex.CreatedAt)
                .HasDefaultValueSql("getutcdate()");
                exercise.Property(ex => ex.HintCode).HasDefaultValue("");
            });

            modelBuilder.Entity<Submission>(sub =>
            {
                sub.Property(item => item.CreatedAt)
                .HasDefaultValueSql("getutcdate()");
            });

            modelBuilder.Entity<Comment>(comment =>
            {
                comment.Property(item => item.CreatedAt)
                        .HasDefaultValueSql("getutcdate()");

                comment.Property(item => item.Upvote)
                        .HasDefaultValue(0);

                comment.Property(item => item.Downvote)
                        .HasDefaultValue(0);
            });

            modelBuilder.Entity<Rating>(rating =>
            {
                rating.Property(item => item.CreatedAt).HasDefaultValueSql("getutcdate()");
            });
            
        }
    }
}
