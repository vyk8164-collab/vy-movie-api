using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
public class ReviewController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReviewController(AppDbContext context)
    {
        _context = context;
    }

    // ========================
    // 📄 GET REVIEW THEO MOVIE
    // ========================
    [HttpGet("movie/{movieId}")]
    public async Task<IActionResult> GetByMovie(int movieId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.MovieId == movieId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new
            {
                r.Id,
                r.Rating,
                r.Comment,
                r.CreatedAt,
                User = r.User.Username
            })
            .ToListAsync();

        return Ok(new { success = true, data = reviews });
    }

    // ========================
    // ⭐ ADD REVIEW
    // ========================
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] ReviewRequest req)
    {
        try
        {
            // ✅ FIX CHUẨN
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdStr == null)
                return Unauthorized(new { success = false, message = "Token không hợp lệ" });

            int userId = int.Parse(userIdStr);

            if (req == null || req.MovieId <= 0)
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            var movieExists = await _context.Movies
                .AnyAsync(m => m.Id == req.MovieId && !m.IsDeleted);

            if (!movieExists)
                return BadRequest(new { success = false, message = "Movie không tồn tại" });

            var exists = await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.MovieId == req.MovieId);

            if (exists)
                return BadRequest(new { success = false, message = "Bạn đã review rồi" });

            var review = new Review
            {
                MovieId = req.MovieId,
                UserId = userId,
                Rating = req.Rating,
                Comment = req.Comment,
                CreatedAt = DateTime.Now
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            await UpdateMovieRating(req.MovieId);

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = ex.Message
            });
        }
    }

    // ========================
    // ✏️ UPDATE
    // ========================
    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ReviewRequest req)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        int userId = int.Parse(userIdStr);

        var review = await _context.Reviews.FindAsync(id);

        if (review == null || review.UserId != userId)
            return NotFound(new { success = false });

        review.Rating = req.Rating;
        review.Comment = req.Comment;

        await _context.SaveChangesAsync();
        await UpdateMovieRating(review.MovieId);

        return Ok(new { success = true });
    }

    // ========================
    // 🗑️ DELETE
    // ========================
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        int userId = int.Parse(userIdStr);

        var review = await _context.Reviews.FindAsync(id);

        if (review == null || review.UserId != userId)
            return NotFound(new { success = false });

        int movieId = review.MovieId;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        await UpdateMovieRating(movieId);

        return Ok(new { success = true });
    }

    // ========================
    // 🔄 UPDATE RATING
    // ========================
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

// DTO
public class ReviewRequest
{
    public int MovieId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; }
}