using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        // 🔥 THÊM DÒNG NÀY
        public string Role { get; set; } = "User";

        public ICollection<Review>? Reviews { get; set; }
        public bool IsLocked { get; set; } = false;
    }
}