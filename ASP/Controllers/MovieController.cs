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

    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        var movies = _context.Movies
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.Reviews)
                .ThenInclude(r => r.User) // 👉 thêm luôn cho chuẩn
            .ToList();

        return Ok(movies);
    }

    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var movie = _context.Movies
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.Reviews).ThenInclude(r => r.User)
            .FirstOrDefault(m => m.Id == id);

        if (movie == null) return NotFound();

        return Ok(movie);
    }

    // CREATE
    [HttpPost]
    public IActionResult Create(Movie movie)
    {
        movie.CreatedBy = "admin";
        movie.CreatedDate = DateTime.Now;

        movie.UpdatedBy = "admin";
        movie.UpdatedDate = DateTime.Now;

        _context.Movies.Add(movie);
        _context.SaveChanges();

        return Ok(movie);
    }

    // UPDATE
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

        movie.UpdatedBy = "admin";
        movie.UpdatedDate = DateTime.Now;

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