using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    // GET theo movie
    [HttpGet("movie/{movieId}")]
    public async Task<IActionResult> GetByMovie(int movieId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .Include(r => r.User)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                User = r.User.Username // ❌ không trả password
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách review thành công",
            data = reviews
        });
    }

    // POST review (có transaction)
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] Review review)
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

        var userExists = await _context.Users
            .AnyAsync(u => u.Id == review.UserId);

        if (!userExists)
        {
            return BadRequest(new
            {
                success = false,
                message = "User không tồn tại",
                data = (object?)null
            });
        }

        var movieExists = await _context.Movies
            .AnyAsync(m => m.Id == review.MovieId && !m.IsDeleted);

        if (!movieExists)
        {
            return BadRequest(new
            {
                success = false,
                message = "Movie không tồn tại",
                data = (object?)null
            });
        }

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            review.CreatedAt = DateTime.Now;

            await _context.Reviews.AddAsync(review);
            await _context.SaveChangesAsync();

            await UpdateMovieRating(review.MovieId);

            await transaction.CommitAsync();

            return Ok(new
            {
                success = true,
                message = "Thêm review thành công",
                data = review
            });
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                success = false,
                message = "Lỗi server",
                data = (object?)null
            });
        }
    }

    // DELETE review theo user (có transaction)
    [HttpDelete("by-user/{userId}")]
    public async Task<IActionResult> DeleteByUser(int userId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.UserId == userId)
            .ToListAsync();

        if (!reviews.Any())
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy review",
                data = (object?)null
            });
        }

        var movieIds = reviews
            .Select(r => r.MovieId)
            .Distinct()
            .ToList();

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Reviews.RemoveRange(reviews);
            await _context.SaveChangesAsync();

            foreach (var movieId in movieIds)
            {
                await UpdateMovieRating(movieId);
            }

            await transaction.CommitAsync();

            return Ok(new
            {
                success = true,
                message = "Xóa review thành công",
                data = reviews
            });
        }
        catch
        {
            await transaction.RollbackAsync();

            return StatusCode(500, new
            {
                success = false,
                message = "Lỗi server",
                data = (object?)null
            });
        }
    }

    // 🔥 UPDATE RATING (tối ưu async)
    private async Task UpdateMovieRating(int movieId)
    {
        var movie = await _context.Movies.FindAsync(movieId);

        if (movie != null)
        {
            movie.RatingAvg = await _context.Reviews
                .Where(r => r.MovieId == movieId)
                .Select(r => (double?)r.Rating)
                .AverageAsync() ?? 0;

            await _context.SaveChangesAsync();
        }
    }
}