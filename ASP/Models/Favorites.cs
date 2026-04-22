using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("Favorites")]
    public class Favorite
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }
        public int MovieId { get; set; }

        public User? User { get; set; }
        public Movie? Movie { get; set; }
    }
}