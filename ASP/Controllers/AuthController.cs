using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // REGISTER
    [HttpPost("register")]
    public IActionResult Register(User user)
    {
        var exists = _context.Users
            .Any(u => u.Username == user.Username);

        if (exists)
            return BadRequest("Username đã tồn tại");

        // 🔥 HASH PASSWORD
        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        _context.Users.Add(user);
        _context.SaveChanges();

        return Ok(user);
    }

    // LOGIN
    [HttpPost("login")]
    public IActionResult Login(User login)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Username == login.Username);

        if (user == null)
            return Unauthorized("Sai tài khoản hoặc mật khẩu");

        // 🔥 SO SÁNH PASSWORD
        bool isValid = BCrypt.Net.BCrypt.Verify(login.Password, user.Password);

        if (!isValid)
            return Unauthorized("Sai tài khoản hoặc mật khẩu");

        return Ok(user);
    }
}