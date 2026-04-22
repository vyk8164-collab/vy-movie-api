using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Movies")]
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime ReleaseDate { get; set; }

        public int Duration { get; set; }

        public double RatingAvg { get; set; }

        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public string? VideoUrl { get; set; }

        // 🔥 VIEW
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public int DislikeCount { get; set; } = 0;

        // 🔥 RELATIONS
        public ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
        public ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        // 🔥 LIKE / DISLIKE (CHUẨN)
        public ICollection<MovieReaction> Reactions { get; set; } = new List<MovieReaction>();

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        public bool IsDeleted { get; set; } = false;
    }
}