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

    public OrdersController(StoreContext context)
    {
        _context = context;
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

        var order = new Order
        {
            UserId = dto.UserId,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        decimal totalPrice = 0;

        foreach (var item in dto.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return BadRequest(new { message = $"Produit {item.ProductId} non trouvé" });
            }

            if (product.Stock < item.Quantity)
            {
                return BadRequest(new { message = $"Stock insuffisant pour {product.Name}" });
            }

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = item.Quantity,
                UnitPrice = product.Price
            };

            order.Items.Add(orderItem);
            totalPrice += product.Price * item.Quantity;

            // Réduire le stock
            product.Stock -= item.Quantity;
            _context.Products.Update(product);
        }

        order.TotalPrice = totalPrice;
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
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
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound(new { message = "Commande non trouvée" });
        }

        if (order.Status == OrderStatus.Cancelled)
        {
            return BadRequest(new { message = "Cette commande est déjà annulée" });
        }

        // Restaurer le stock
        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock += item.Quantity;
                _context.Products.Update(product);
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Commande annulée avec succès" });
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
