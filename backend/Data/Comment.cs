using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace backend.Data
{
    [Table("Comment")]
    public class Comment
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public Users Users { get; set; }

        public string Content {  get; set; }

        public Guid ExerciseId { get; set; }

        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; }

        public int Upvote {  get; set; }

        public int Downvote { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
