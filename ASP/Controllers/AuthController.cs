using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthController(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    // REGISTER
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
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

        var exists = await _context.Users
            .AnyAsync(u => u.Username == user.Username);

        if (exists)
        {
            return BadRequest(new
            {
                success = false,
                message = "Username đã tồn tại",
                data = (object?)null
            });
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Đăng ký thành công",
            data = new
            {
                user.Id,
                user.Username
            }
        });
    }

    // LOGIN + JWT
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User login)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == login.Username);

        if (user == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Sai tài khoản hoặc mật khẩu",
                data = (object?)null
            });
        }

        bool isValid = BCrypt.Net.BCrypt.Verify(login.Password, user.Password);

        if (!isValid)
        {
            return Unauthorized(new
            {
                success = false,
                message = "Sai tài khoản hoặc mật khẩu",
                data = (object?)null
            });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            message = "Đăng nhập thành công",
            data = new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Username
                }
            }
        });
    }

    // 🔥 GENERATE JWT
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}