using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetiteMaisonEpouvante.API.Data;
using PetiteMaisonEpouvante.Core;

namespace PetiteMaisonEpouvante.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrdersController : ControllerBase
{
    private readonly StoreContext _context;
    private readonly OrderService _orderService;

    public OrdersController(StoreContext context, OrderService orderService)
    {
        _context = context;
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders([FromQuery] string? userId = null)
    {
        var query = _context.Orders.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(userId))
        {
            query = query.Where(o => o.UserId == userId);
        }

        var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Commande non trouvée" });
        }

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder([FromBody] CreateOrderDto dto)
    {
        if (!ModelState.IsValid || dto.Items == null || !dto.Items.Any())
        {
            return BadRequest(new { message = "Données de commande invalides" });
        }

        try
        {
            var order = await _orderService.CreateOrderAsync(dto);
            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound(new { message = "Commande non trouvée" });
        }

        order.Status = dto.Status;
        order.UpdatedAt = DateTime.UtcNow;

        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return Ok(order);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelOrder(Guid id)
    {
        try
        {
            await _orderService.CancelOrderAsync(id);
            return Ok(new { message = "Commande annulée avec succès" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class CreateOrderDto
{
    public string UserId { get; set; } = string.Empty;
    public List<OrderItemDto>? Items { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
}
