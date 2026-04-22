using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ConnectDB.Data;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var totalMovies = await _context.Movies.CountAsync();
        var totalUsers = await _context.Users.CountAsync();
        var totalReviews = await _context.Reviews.CountAsync();

        return Ok(new
        {
            success = true,
            data = new
            {
                totalMovies,
                totalUsers,
                totalReviews
            }
        });
    }
}