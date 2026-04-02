using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Genres")]
    public class Genre
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<MovieGenre>? MovieGenres { get; set; }

        // 👉 AUDIT
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}