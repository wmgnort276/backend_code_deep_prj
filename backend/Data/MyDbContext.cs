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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Exercise>(exercise =>
            {
                exercise.Property(ex => ex.CreatedAt)
                .HasDefaultValueSql("getutcdate()");
            });
        }
    }
}
