using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
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

    // ========================
    // GET ALL (PUBLIC)
    // ========================
    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int pageSize = 10,
        string? search = null,
        int? genreId = null)
    {
        var query = _context.Movies
            .Where(m => !m.IsDeleted)
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(m => m.Title.Contains(search));
        }

        // FILTER GENRE
        if (genreId.HasValue)
        {
            query = query.Where(m =>
                m.MovieGenres.Any(g => g.GenreId == genreId));
        }

        var movies = await query
            .Include(m => m.MovieGenres)
                .ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieActors)
                .ThenInclude(ma => ma.Actor)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.PosterUrl,
                m.RatingAvg,
                Genres = m.MovieGenres.Select(g => g.Genre.Name),
                Actors = m.MovieActors.Select(a => a.Actor.Name)
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách movie thành công",
            data = movies
        });
    }

    // ========================
    // GET BY ID (PUBLIC)
    // ========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _context.Movies
            .Where(m => m.Id == id && !m.IsDeleted)
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.Reviews).ThenInclude(r => r.User)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.Description,
                m.ReleaseDate,
                m.Duration,
                m.RatingAvg,
                m.PosterUrl,
                m.TrailerUrl,

                Actors = m.MovieActors.Select(a => a.Actor.Name),
                Genres = m.MovieGenres.Select(g => g.Genre.Name),

                Reviews = m.Reviews.Select(r => new
                {
                    r.Id,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt,
                    User = r.User.Username
                })
            })
            .FirstOrDefaultAsync();

        if (movie == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy movie",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Lấy dữ liệu thành công",
            data = movie
        });
    }

    // ========================
    // CREATE (ADMIN ONLY)
    // ========================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Movie movie)
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

        movie.CreatedBy = "admin";
        movie.CreatedDate = DateTime.Now;
        movie.UpdatedBy = "admin";
        movie.UpdatedDate = DateTime.Now;
        movie.IsDeleted = false;

        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Tạo movie thành công",
            data = movie
        });
    }

    // ========================
    // UPDATE (ADMIN ONLY)
    // ========================
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Movie updated)
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

        var movie = await _context.Movies.FindAsync(id);

        if (movie == null || movie.IsDeleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy movie",
                data = (object?)null
            });
        }

        movie.Title = updated.Title;
        movie.Description = updated.Description;
        movie.ReleaseDate = updated.ReleaseDate;
        movie.Duration = updated.Duration;
        movie.PosterUrl = updated.PosterUrl;
        movie.TrailerUrl = updated.TrailerUrl;

        movie.UpdatedBy = "admin";
        movie.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Cập nhật movie thành công",
            data = movie
        });
    }

    // ========================
    // DELETE (ADMIN ONLY)
    // ========================
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null || movie.IsDeleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy movie",
                data = (object?)null
            });
        }

        movie.IsDeleted = true;
        movie.UpdatedBy = "admin";
        movie.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Xóa movie thành công",
            data = movie
        });
    }
}