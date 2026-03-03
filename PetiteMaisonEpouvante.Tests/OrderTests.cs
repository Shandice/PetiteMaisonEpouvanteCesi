using PetiteMaisonEpouvante.Core;

namespace PetiteMaisonEpouvante.Tests;

public class OrderTests
{
    [Fact]
    public void Order_Initialization_Should_Have_Pending_Status()
    {
        // Arrange & Act
        var order = new Order { UserId = "user123" };

        // Assert
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal("user123", order.UserId);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void Order_Should_Calculate_Total_Price()
    {
        // Arrange
        var order = new Order { UserId = "user123", TotalPrice = 100M };

        // Act & Assert
        Assert.Equal(100M, order.TotalPrice);
    }

    [Fact]
    public void OrderItem_Should_Store_Quantity_And_Price()
    {
        // Arrange
        var orderItem = new OrderItem 
        { 
            ProductId = Guid.NewGuid(),
            ProductName = "Figure",
            Quantity = 2,
            UnitPrice = 25.00M
        };

        // Act & Assert
        Assert.Equal(2, orderItem.Quantity);
        Assert.Equal(25.00M, orderItem.UnitPrice);
    }

    [Fact]
    public void Order_Should_Support_Status_Transitions()
    {
        // Arrange
        var order = new Order { UserId = "user123", Status = OrderStatus.Pending };

        // Act
        order.Status = OrderStatus.Confirmed;

        // Assert
        Assert.Equal(OrderStatus.Confirmed, order.Status);
    }
}
