using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;

[ApiController]
[Route("api/[controller]")]
public class MovieController : ControllerBase
{
    private readonly AppDbContext _context;

    public MovieController(AppDbContext context)
    {
        _context = context;
    }

    // GET all (kèm actor + genre + review)
    [HttpGet]
    public IActionResult GetAll()
    {
        var movies = _context.Movies
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.Reviews)
            .ToList();

        return Ok(movies);
    }

    // GET by id
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var movie = _context.Movies
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.Reviews)
    .ThenInclude(r => r.User);

        if (movie == null) return NotFound();

        return Ok(movie);
    }

    // POST
    [HttpPost]
    public IActionResult Create(Movie movie)
    {
        _context.Movies.Add(movie);
        _context.SaveChanges();
        return Ok(movie);
    }

    // PUT
    [HttpPut("{id}")]
    public IActionResult Update(int id, Movie updated)
    {
        var movie = _context.Movies.Find(id);
        if (movie == null) return NotFound();

        movie.Title = updated.Title;
        movie.Description = updated.Description;
        movie.ReleaseDate = updated.ReleaseDate;
        movie.Duration = updated.Duration;
        movie.PosterUrl = updated.PosterUrl;
        movie.TrailerUrl = updated.TrailerUrl;

        _context.SaveChanges();
        return Ok(movie);
    }

    // DELETE
    [HttpDelete("by-title/{title}")]
    public IActionResult DeleteByTitle(string title)
    {
        var movie = _context.Movies
            .FirstOrDefault(m => m.Title == title);

        if (movie == null) return NotFound();

        _context.Movies.Remove(movie);
        _context.SaveChanges();

        return Ok();
    }
}