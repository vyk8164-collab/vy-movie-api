using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;

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
    public IActionResult GetByMovie(int movieId)
    {
        var reviews = _context.Reviews
            .Where(r => r.MovieId == movieId)
            .ToList();

        return Ok(reviews);
    }

    // POST review
    [HttpPost]
    public IActionResult AddReview(Review review)
    {
        review.CreatedAt = DateTime.Now;

        var userExists = _context.Users.Any(u => u.Id == review.UserId);
        if (!userExists)
            return BadRequest("User không tồn tại");

        _context.Reviews.Add(review);
        _context.SaveChanges();

        UpdateMovieRating(review.MovieId);

        return Ok(review);
    }
    // DELETE review
    [HttpDelete("by-user/{userId}")]
    public IActionResult DeleteByUser(int userId)
    {
        var reviews = _context.Reviews
            .Where(r => r.UserId == userId)
            .ToList();

        if (!reviews.Any()) return NotFound();

        var movieIds = reviews.Select(r => r.MovieId).Distinct().ToList();

        _context.Reviews.RemoveRange(reviews);
        _context.SaveChanges();

        // 🔥 update lại rating cho các phim bị ảnh hưởng
        foreach (var movieId in movieIds)
        {
            UpdateMovieRating(movieId);
        }

        return Ok();
    }

    // 🔥 FUNCTION TÍNH RATING
    private void UpdateMovieRating(int movieId)
    {
        var movie = _context.Movies.Find(movieId);

        if (movie != null)
        {
            movie.RatingAvg = _context.Reviews
                .Where(r => r.MovieId == movieId)
                .Select(r => (double?)r.Rating)
                .Average() ?? 0;

            _context.SaveChanges();
        }
    }
}