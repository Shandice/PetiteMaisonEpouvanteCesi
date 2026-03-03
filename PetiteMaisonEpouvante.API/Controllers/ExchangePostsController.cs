using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetiteMaisonEpouvante.API.Data;
using PetiteMaisonEpouvante.Core;

namespace PetiteMaisonEpouvante.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExchangePostsController : ControllerBase
{
    private readonly StoreContext _context;

    public ExchangePostsController(StoreContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExchangePost>>> GetExchangePosts(
        [FromQuery] ExchangeType? type = null,
        [FromQuery] bool? activeOnly = true)
    {
        var query = _context.ExchangePosts.AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(ep => ep.Type == type);
        }

        if (activeOnly.HasValue && activeOnly.Value)
        {
            query = query.Where(ep => ep.IsActive);
        }

        var posts = await query.OrderByDescending(ep => ep.CreatedAt).ToListAsync();
        return Ok(posts);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ExchangePost>> GetExchangePost(Guid id)
    {
        var post = await _context.ExchangePosts.FindAsync(id);

        if (post == null)
        {
            return NotFound(new { message = "Annonce d'échange non trouvée" });
        }

        return Ok(post);
    }

    [HttpPost]
    public async Task<ActionResult<ExchangePost>> CreateExchangePost([FromBody] CreateExchangePostDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var post = new ExchangePost
        {
            UserId = dto.UserId,
            UserName = dto.UserName,
            Title = dto.Title,
            Description = dto.Description,
            PhotoUrl = dto.PhotoUrl,
            Type = dto.Type,
            Tags = dto.Tags ?? new List<string>(),
            IsActive = true
        };

        _context.ExchangePosts.Add(post);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetExchangePost), new { id = post.Id }, post);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateExchangePost(Guid id, [FromBody] UpdateExchangePostDto dto)
    {
        var post = await _context.ExchangePosts.FindAsync(id);

        if (post == null)
        {
            return NotFound(new { message = "Annonce d'échange non trouvée" });
        }

        post.Title = dto.Title ?? post.Title;
        post.Description = dto.Description ?? post.Description;
        post.PhotoUrl = dto.PhotoUrl ?? post.PhotoUrl;
        post.Tags = dto.Tags ?? post.Tags;
        post.UpdatedAt = DateTime.UtcNow;

        _context.ExchangePosts.Update(post);
        await _context.SaveChangesAsync();

        return Ok(post);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteExchangePost(Guid id)
    {
        var post = await _context.ExchangePosts.FindAsync(id);

        if (post == null)
        {
            return NotFound(new { message = "Annonce d'échange non trouvée" });
        }

        _context.ExchangePosts.Remove(post);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/deactivate")]
    public async Task<IActionResult> DeactivateExchangePost(Guid id)
    {
        var post = await _context.ExchangePosts.FindAsync(id);

        if (post == null)
        {
            return NotFound(new { message = "Annonce d'échange non trouvée" });
        }

        post.IsActive = false;
        post.UpdatedAt = DateTime.UtcNow;
        _context.ExchangePosts.Update(post);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Annonce désactivée" });
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<ExchangePost>>> SearchExchangePosts(
        [FromQuery] string query,
        [FromQuery] List<string>? tags = null)
    {
        var posts = _context.ExchangePosts
            .Where(ep => ep.IsActive && (ep.Title.Contains(query) || ep.Description.Contains(query)))
            .AsQueryable();

        if (tags?.Any() == true)
        {
            posts = posts.Where(ep => ep.Tags.Any(t => tags.Contains(t)));
        }

        var results = await posts.OrderByDescending(ep => ep.CreatedAt).ToListAsync();
        return Ok(results);
    }
}

public class CreateExchangePostDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public ExchangeType Type { get; set; }
    public List<string>? Tags { get; set; }
}

public class UpdateExchangePostDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? PhotoUrl { get; set; }
    public List<string>? Tags { get; set; }
}
