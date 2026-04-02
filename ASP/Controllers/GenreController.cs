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

    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Genres.ToList());
    }

    // GET BY ID (thêm cho chuẩn)
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var genre = _context.Genres.Find(id);
        if (genre == null) return NotFound();
        return Ok(genre);
    }

    // CREATE (có audit)
    [HttpPost]
    public IActionResult Create(Genre genre)
    {
        genre.CreatedBy = "admin";
        genre.CreatedDate = DateTime.Now;

        genre.UpdatedBy = "admin";
        genre.UpdatedDate = DateTime.Now;

        _context.Genres.Add(genre);
        _context.SaveChanges();

        return Ok(genre);
    }

    // UPDATE (thêm cho đủ CRUD)
    [HttpPut("{id}")]
    public IActionResult Update(int id, Genre updated)
    {
        var genre = _context.Genres.Find(id);
        if (genre == null) return NotFound();

        genre.Name = updated.Name;

        // audit
        genre.UpdatedBy = "admin";
        genre.UpdatedDate = DateTime.Now;

        _context.SaveChanges();

        return Ok(genre);
    }

    // DELETE
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