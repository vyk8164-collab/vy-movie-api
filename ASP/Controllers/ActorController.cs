using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ActorController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActorController(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL + Pagination
    [HttpGet]
    public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
    {
        var actors = await _context.Actors
            .Where(a => !a.IsDeleted)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            success = true,
            message = "Lấy danh sách actor thành công",
            data = actors
        });
    }

    // GET BY ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var actor = await _context.Actors
            .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

        if (actor == null)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy actor",
                data = (object?)null
            });
        }

        return Ok(new
        {
            success = true,
            message = "Lấy dữ liệu thành công",
            data = actor
        });
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Actor actor)
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

        actor.CreatedBy = "admin";
        actor.CreatedDate = DateTime.Now;
        actor.UpdatedBy = "admin";
        actor.UpdatedDate = DateTime.Now;
        actor.IsDeleted = false;

        await _context.Actors.AddAsync(actor);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Tạo actor thành công",
            data = actor
        });
    }

    // UPDATE
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Actor updated)
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

        var actor = await _context.Actors.FindAsync(id);

        if (actor == null || actor.IsDeleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy actor",
                data = (object?)null
            });
        }

        actor.Name = updated.Name;
        actor.DateOfBirth = updated.DateOfBirth;
        actor.Bio = updated.Bio;
        actor.AvatarUrl = updated.AvatarUrl;

        actor.UpdatedBy = "admin";
        actor.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Cập nhật actor thành công",
            data = actor
        });
    }

    // SOFT DELETE
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var actor = await _context.Actors.FindAsync(id);

        if (actor == null || actor.IsDeleted)
        {
            return NotFound(new
            {
                success = false,
                message = "Không tìm thấy actor",
                data = (object?)null
            });
        }

        actor.IsDeleted = true;
        actor.UpdatedBy = "admin";
        actor.UpdatedDate = DateTime.Now;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Xóa actor thành công",
            data = actor
        });
    }
}