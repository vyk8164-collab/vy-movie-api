using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("MovieGenres")]
    public class MovieGenre
    {
        public int MovieId { get; set; }

        public int GenreId { get; set; }
    }
}