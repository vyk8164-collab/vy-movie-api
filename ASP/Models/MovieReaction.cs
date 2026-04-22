using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("MovieReactions")]
    public class MovieReaction
    {
        [Key]
        public int Id { get; set; }

        public int MovieId { get; set; }
        public int UserId { get; set; }

        // 👍 like = true | 👎 dislike = false
        public bool IsLike { get; set; }

        public Movie? Movie { get; set; }
        public User? User { get; set; }
    }
}