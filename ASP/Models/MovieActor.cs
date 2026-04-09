using System.ComponentModel.DataAnnotations.Schema;

namespace ConnectDB.Models
{
    [Table("MovieActors")]
    public class MovieActor
    {
        public int MovieId { get; set; }
        public int ActorId { get; set; }

        // navigation
        public Movie? Movie { get; set; }
        public Actor? Actor { get; set; }
    }
}