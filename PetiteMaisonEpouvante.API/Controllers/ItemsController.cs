using PetiteMaisonEpouvante.API.Data;
using PetiteMaisonEpouvante.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetiteMaisonEpouvante.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ItemsController : ControllerBase
{
    private readonly StoreContext _context;
    private readonly ILogger<ItemsController> _logger;
    private readonly IConfiguration _config;

    // Injection de IConfiguration
    public ItemsController(StoreContext context, ILogger<ItemsController> logger, IConfiguration config)
    {
        _context = context;
        _logger = logger;
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _context.Items.ToListAsync());
    }

    [HttpPost("login")]
    public IActionResult Login()
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        // Récupération de la clé depuis appsettings.json
        var secretKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing");
        var key = Encoding.ASCII.GetBytes(secretKey);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim("id", "user_1") }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return Ok(new { Token = tokenHandler.WriteToken(token) });
    }

    // Endpoint protégé : Seul un utilisateur avec un Token peut créer un article
    [Authorize] 
    [HttpPost]
    public async Task<IActionResult> CreateItem([FromBody] Item item)
    {
        if (item.Price <= 0) 
        {
            _logger.LogWarning("Tentative création prix invalide");
            return BadRequest("Prix invalide");
        }
        
        // On force un ID vendeur fictif pour le POC
        item.SellerId = User.FindFirst("id")?.Value ?? "unknown";

        _context.Items.Add(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Nouvel article créé : {item.Name} par {item.SellerId}");
        return CreatedAtAction(nameof(GetAll), new { id = item.Id }, item);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItem(Guid id)
    {
        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        _context.Items.Remove(item);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation($"Article supprimé : {item.Name} (ID: {id})");
        return NoContent();
    }
}