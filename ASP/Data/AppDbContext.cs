using Microsoft.EntityFrameworkCore;
using ConnectDB.Models;

namespace ConnectDB.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; }
    public DbSet<Actor> Actors { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<MovieActor> MovieActors { get; set; }
    public DbSet<MovieGenre> MovieGenres { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite key cho bảng trung gian
        modelBuilder.Entity<MovieActor>()
            .HasKey(ma => new { ma.MovieId, ma.ActorId });

        modelBuilder.Entity<MovieGenre>()
            .HasKey(mg => new { mg.MovieId, mg.GenreId });
    }
}