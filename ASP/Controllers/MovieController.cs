using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ConnectDB.Data;
using ConnectDB.Models;
using ConnectDB.Models.DTOs;
using ConnectDB.Utils;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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
    // 🔥 CREATE
    // ========================
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovieDto data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.Title))
            return BadRequest(new { success = false });

        var videoEmbed = YouTubeHelper.ToEmbedUrl(data.VideoUrl);
        var trailerEmbed = YouTubeHelper.ToEmbedUrl(data.TrailerUrl);
        var poster = ResolvePoster(data.PosterUrl, videoEmbed);

        var movie = new Movie
        {
            Title = data.Title,
            Description = data.Description,
            ReleaseDate = data.ReleaseDate ?? DateTime.Now,
            Duration = data.Duration ?? 0,
            PosterUrl = poster,
            VideoUrl = videoEmbed,
            TrailerUrl = trailerEmbed,
            ViewCount = 0,
            CreatedBy = "admin",
            CreatedDate = DateTime.Now
        };

        _context.Movies.Add(movie);
        await _context.SaveChangesAsync();

        return Ok(new { success = true, data = movie });
    }

    // ========================
    // ✏️ UPDATE (🔥 FIX 405)
    // ========================
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateMovieDto data)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieGenres)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movie == null)
            return NotFound(new { success = false });

        movie.Title = data.Title;
        movie.Description = data.Description;
        movie.Duration = data.Duration ?? 0;
        movie.VideoUrl = YouTubeHelper.ToEmbedUrl(data.VideoUrl);
        movie.TrailerUrl = YouTubeHelper.ToEmbedUrl(data.TrailerUrl);

        if (!string.IsNullOrWhiteSpace(data.PosterUrl))
            movie.PosterUrl = data.PosterUrl;

        // 🔥 update genres
        movie.MovieGenres.Clear();

        if (data.GenreIds != null)
        {
            foreach (var gid in data.GenreIds)
            {
                movie.MovieGenres.Add(new MovieGenre
                {
                    MovieId = id,
                    GenreId = gid
                });
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ========================
    // 📄 GET ALL
    // ========================
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var query = _context.Movies
            .Where(m => !m.IsDeleted)
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .Include(m => m.Reviews)
            .Include(m => m.Reactions);

        var totalItems = await query.CountAsync();

        var movies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new
            {
                m.Id,
                m.Title,
                m.PosterUrl,
                m.Duration,
                m.RatingAvg,
                m.ViewCount,
                m.VideoUrl,

                ReviewCount = m.Reviews.Count(),

                LikeCount = m.Reactions.Count(r => r.IsLike),
                DislikeCount = m.Reactions.Count(r => !r.IsLike),

                Genres = m.MovieGenres.Select(g => g.Genre.Name),
                Actors = m.MovieActors.Select(a => a.Actor.Name)
            })
            .ToListAsync();

        return Ok(new
        {
            success = true,
            data = movies,
            pagination = new
            {
                page,
                pageSize,
                totalItems,
                totalPages = (int)Math.Ceiling((double)totalItems / pageSize)
            }
        });
    }

    // ========================
    // 🎬 GET BY ID
    // ========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _context.Movies
            .Include(m => m.MovieGenres).ThenInclude(mg => mg.Genre)
            .Include(m => m.MovieActors).ThenInclude(ma => ma.Actor)
            .Include(m => m.Reactions)
            .Include(m => m.Reviews).ThenInclude(r => r.User)
            .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);

        if (movie == null)
            return NotFound(new { success = false });

        return Ok(new
        {
            success = true,
            data = new
            {
                movie.Id,
                movie.Title,
                movie.Description,
                movie.PosterUrl,
                movie.VideoUrl,
                movie.TrailerUrl,
                movie.Duration,
                movie.RatingAvg,
                movie.ViewCount,

                LikeCount = movie.Reactions.Count(r => r.IsLike),
                DislikeCount = movie.Reactions.Count(r => !r.IsLike),
                ReviewCount = movie.Reviews.Count(),

                Reviews = movie.Reviews.Select(r => new
                {
                    r.Id,
                    r.Comment,
                    r.Rating,
                    User = r.User != null ? r.User.Username : "Ẩn danh",
                    r.CreatedAt
                }),

                Genres = movie.MovieGenres.Select(g => g.Genre.Name),
                Actors = movie.MovieActors.Select(a => a.Actor.Name)
            }
        });
    }

    // ========================
    // 👁 VIEW
    // ========================
    [HttpPost("{id}/view")]
    public async Task<IActionResult> AddView(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
            return NotFound(new { success = false });

        movie.ViewCount++;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ========================
    // 👍 REACT (🔥 FIX 500)
    // ========================
    [Authorize]
    [HttpPost("{id}/react")]
    public async Task<IActionResult> React(int id, bool isLike)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        int userId = int.Parse(userIdStr);

        var movieExists = await _context.Movies.AnyAsync(x => x.Id == id);
        if (!movieExists)
            return NotFound(new { success = false });

        var existing = await _context.MovieReactions
            .FirstOrDefaultAsync(x => x.MovieId == id && x.UserId == userId);

        if (existing != null)
        {
            existing.IsLike = isLike;
        }
        else
        {
            _context.MovieReactions.Add(new MovieReaction
            {
                MovieId = id,
                UserId = userId,
                IsLike = isLike
            });
        }

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ========================
    // ❤️ FAVORITE
    // ========================
    [Authorize]
    [HttpPost("{id}/favorite")]
    public async Task<IActionResult> ToggleFavorite(int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        int userId = int.Parse(userIdStr);

        var fav = await _context.Favorites
            .FirstOrDefaultAsync(f => f.MovieId == id && f.UserId == userId);

        if (fav != null)
            _context.Favorites.Remove(fav);
        else
            _context.Favorites.Add(new Favorite { MovieId = id, UserId = userId });

        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ========================
    // ⭐ REVIEW (🔥 FIX FK ERROR)
    // ========================
    [Authorize]
    [HttpPost("{id}/review")]
    public async Task<IActionResult> AddReview(int id, [FromBody] Review data)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdStr == null)
            return Unauthorized();

        int userId = int.Parse(userIdStr);

        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            return BadRequest(new { success = false, message = "User không tồn tại" });

        data.MovieId = id;
        data.UserId = userId;
        data.CreatedAt = DateTime.Now;

        _context.Reviews.Add(data);
        await _context.SaveChangesAsync();

        var avg = await _context.Reviews
            .Where(r => r.MovieId == id)
            .AverageAsync(r => r.Rating);

        var movie = await _context.Movies.FindAsync(id);
        if (movie != null)
        {
            movie.RatingAvg = avg;
            await _context.SaveChangesAsync();
        }

        return Ok(new { success = true });
    }

    // ========================
    // ❌ DELETE
    // ========================
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await _context.Movies.FindAsync(id);

        if (movie == null)
            return NotFound(new { success = false });

        movie.IsDeleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { success = true });
    }

    // ========================
    // HELPER
    // ========================
    private string ResolvePoster(string? posterUrl, string? videoEmbed)
    {
        if (!string.IsNullOrWhiteSpace(posterUrl) && posterUrl.StartsWith("http"))
            return posterUrl;

        if (!string.IsNullOrEmpty(videoEmbed))
        {
            var videoId = YouTubeHelper.GetVideoId(videoEmbed);
            if (!string.IsNullOrEmpty(videoId))
                return $"https://img.youtube.com/vi/{videoId}/hqdefault.jpg"; // 🔥 fix 404
        }

        return "https://dummyimage.com/300x450/cccccc/000000&text=No+Image";
    }
}