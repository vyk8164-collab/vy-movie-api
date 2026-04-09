using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Actors")]
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên không được để trống")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public ICollection<MovieActor>? MovieActors { get; set; }

        // AUDIT
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }

        // SOFT DELETE
        public bool IsDeleted { get; set; } = false;
    }
}