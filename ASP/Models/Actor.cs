using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Actors")]
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
    }
}