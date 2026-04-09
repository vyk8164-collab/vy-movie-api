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
        public string? PosterUrl { get; set; }   // ảnh phim
        public string? TrailerUrl { get; set; }  // trailer
        public ICollection<MovieActor>? MovieActors { get; set; }
        public ICollection<MovieGenre>? MovieGenres { get; set; }
        public ICollection<Review>? Reviews { get; set; }
        public string? VideoUrl { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}