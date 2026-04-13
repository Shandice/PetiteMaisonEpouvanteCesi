using Microsoft.EntityFrameworkCore;
using PetiteMaisonEpouvante.API.Data;
using PetiteMaisonEpouvante.API.Controllers;
using PetiteMaisonEpouvante.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PetiteMaisonEpouvante.API;

public class OrderService
{
    private readonly StoreContext _context;

    public OrderService(StoreContext context)
    {
        _context = context;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderDto dto)
    {
        var order = new Order
        {
            UserId = dto.UserId,
            Status = OrderStatus.Pending,
            Items = new List<OrderItem>()
        };

        decimal totalPrice = 0;

        foreach (var item in dto.Items!)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
                throw new Exception($"Produit {item.ProductId} non trouvé");

            if (product.Stock < item.Quantity)
                throw new Exception($"Stock insuffisant pour {product.Name}");

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

        return order;
    }

    public async Task<Order> CancelOrderAsync(Guid id)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == id);
        
        if (order == null)
            throw new KeyNotFoundException("Commande non trouvée");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cette commande est déjà annulée");

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

        return order;
    }
}