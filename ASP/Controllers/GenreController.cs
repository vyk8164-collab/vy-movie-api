using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class GenreController : ControllerBase
{
    private readonly AppDbContext _context;

    public GenreController(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL + Pagination
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var genres = await _context.Genres
            .Where(g => !g.IsDeleted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách genre thành công",
            data = genres
        });
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var genre = await _context.Genres
            .FirstOrDefaultAsync(g => g.Id == id && !g.IsDeleted);

        if (genre == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy genre",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Lấy dữ liệu thành công",
            data = genre
        });
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Genre genre)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                data = ModelState
            });
        }

        genre.CreatedBy = "admin";
        genre.CreatedDate = DateTime.Now;
        genre.UpdatedBy = "admin";
        genre.UpdatedDate = DateTime.Now;
        genre.IsDeleted = false;

        await _context.Genres.AddAsync(genre);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Tạo genre thành công",
            data = genre
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Genre updated)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                success = false,
                message = "Dữ liệu không hợp lệ",
                data = ModelState
            });
        }

        var genre = await _context.Genres.FindAsync(id);

        if (genre == null || genre.IsDeleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy genre",
                data = (object?)null
            });
        }

        genre.Name = updated.Name;
        genre.UpdatedBy = "admin";
        genre.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Cập nhật genre thành công",
            data = genre
        });
    }

    // SOFT DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var genre = await _context.Genres.FindAsync(id);

        if (genre == null || genre.IsDeleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy genre",
                data = (object?)null
            });
        }

        genre.IsDeleted = true;
        genre.UpdatedBy = "admin";
        genre.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Xóa genre thành công",
            data = genre
        });
    }
}