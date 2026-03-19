using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int MovieId { get; set; }

        [Required]
        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; }

    }
}