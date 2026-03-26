using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GenreController : ControllerBase
{
    private readonly AppDbContext _context;

    public GenreController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Genres.ToList());
    }

    [HttpPost]
    public IActionResult Create(Genre genre)
    {
        _context.Genres.Add(genre);
        _context.SaveChanges();
        return Ok(genre);
    }

    [HttpDelete("by-name/{name}")]
    public IActionResult DeleteGenre(string name)
    {
        var genre = _context.Genres
            .FirstOrDefault(g => g.Name == name);

        if (genre == null) return NotFound();

        _context.Genres.Remove(genre);
        _context.SaveChanges();

        return Ok();
    }
}