using ConnectDB.Data;
using ConnectDB.Models;
using ConnectDB.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Google.Apis.Auth;
using ConnectDB.Models.DTOs;

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

    // ========================
    // REGISTER
    // ========================
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { success = false, message = "Thiếu dữ liệu" });
        }

        var exists = await _context.Users
            .AnyAsync(u => u.Username == dto.Username);

        if (exists)
        {
            return BadRequest(new { success = false, message = "Username đã tồn tại" });
        }

        var user = new User
        {
            Username = dto.Username,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            IsLocked = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            data = new { user.Id, user.Username, user.Role }
        });
    }

    // ========================
    // LOGIN
    // ========================
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password))
        {
            return BadRequest(new { success = false, message = "Thiếu dữ liệu" });
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == dto.Username);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
        {
            return Unauthorized(new { success = false, message = "Sai tài khoản hoặc mật khẩu" });
        }

        if (user.IsLocked)
        {
            return Unauthorized(new { success = false, message = "Tài khoản bị khóa" });
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            data = new
            {
                token,
                user = new { user.Id, user.Username, user.Role }
            }
        });
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleDto dto)
    {
        var payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken);

        var email = payload.Email;

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == email);

        if (user == null)
        {
            user = new User
            {
                Username = email,
                Password = "",
                Role = "User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        var token = GenerateJwtToken(user);

        return Ok(new
        {
            success = true,
            data = new
            {
                token,
                id = user.Id,
                username = user.Username
            }
        });
    }

    // ========================
    // JWT
    // ========================
    private string GenerateJwtToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"])
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _config["Jwt:Issuer"],
            _config["Jwt:Audience"],
            claims,
            expires: DateTime.Now.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}