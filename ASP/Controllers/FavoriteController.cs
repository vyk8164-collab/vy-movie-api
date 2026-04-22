using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ConnectDB.Data;
using ConnectDB.Models;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FavoriteController : ControllerBase
{
    private readonly AppDbContext _context;

    public FavoriteController(AppDbContext context)
    {
        _context = context;
    }

    // ========================
    // ❤️ ADD FAVORITE
    // ========================
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] int movieId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.MovieId == movieId);

        if (exists)
        {
            return BadRequest(new
            {
                success = false,
                message = "Đã tồn tại trong danh sách yêu thích"
            });
        }

        await _context.Favorites.AddAsync(new Favorite
        {
            UserId = userId,
            MovieId = movieId
        });

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ========================
    // 📄 GET MY FAVORITES
    // ========================
    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        var data = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Include(f => f.Movie)
            .Select(f => new
            {
                f.MovieId,
                f.Movie.Title,
                f.Movie.PosterUrl,
                f.Movie.RatingAvg
            })
            .ToListAsync();

        return Ok(new { success = true, data });
    }

    // ========================
    // 🔍 CHECK FAVORITE
    // ========================
    [HttpGet("check")]
    public async Task<IActionResult> Check(int movieId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return Unauthorized();

        var userId = int.Parse(userIdClaim.Value);

        var exists = await _context.Favorites
            .AnyAsync(f => f.UserId == userId && f.MovieId == movieId);

        return Ok(new
        {
            success = true,
            isFavorite = exists
        });
    }
}