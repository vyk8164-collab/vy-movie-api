using ConnectDB.Data;
using ConnectDB.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ActorController : ControllerBase
{
    private readonly AppDbContext _context;

    public ActorController(AppDbContext context)
    {
        _context = context;
    }

    // GET ALL
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(_context.Actors.ToList());
    }

    // GET BY ID
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var actor = _context.Actors.Find(id);
        if (actor == null) return NotFound();
        return Ok(actor);
    }

    // ✅ CREATE (có audit)
    [HttpPost]
    public IActionResult Create(Actor actor)
    {
        actor.CreatedBy = "admin";
        actor.CreatedDate = DateTime.Now;

        actor.UpdatedBy = "admin";
        actor.UpdatedDate = DateTime.Now;

        _context.Actors.Add(actor);
        _context.SaveChanges();

        return Ok(actor);
    }

    // ✅ UPDATE (có audit)
    [HttpPut("{id}")]
    public IActionResult Update(int id, Actor updated)
    {
        var actor = _context.Actors.Find(id);
        if (actor == null) return NotFound();

        actor.Name = updated.Name;
        actor.DateOfBirth = updated.DateOfBirth;
        actor.Bio = updated.Bio;
        actor.AvatarUrl = updated.AvatarUrl;

        // 👉 audit
        actor.UpdatedBy = "admin";
        actor.UpdatedDate = DateTime.Now;

        _context.SaveChanges();

        return Ok(actor);
    }

    // DELETE
    [HttpDelete("by-name/{name}")]
    public IActionResult DeleteByName(string name)
    {
        var actor = _context.Actors
            .FirstOrDefault(a => a.Name == name);

        if (actor == null) return NotFound();

        _context.Actors.Remove(actor);
        _context.SaveChanges();

        return Ok();
    }
}