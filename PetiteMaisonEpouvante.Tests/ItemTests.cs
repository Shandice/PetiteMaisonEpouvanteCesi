using PetiteMaisonEpouvante.Core;

namespace PetiteMaisonEpouvante.Tests;

public class ProductTests
{
    [Fact]
    public void Product_Initialization_Should_Set_Default_Values()
    {
        // Arrange & Act
        var product = new Product();

        // Assert
        Assert.NotEqual(Guid.Empty, product.Id);
        Assert.Empty(product.Name);
        Assert.Empty(product.Description);
        Assert.Equal(0, product.Price);
        Assert.Equal(0, product.Stock);
        Assert.True(product.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public void Product_Should_Have_Valid_Price_Range()
    {
        // Arrange & Act
        var product = new Product 
        { 
            Name = "Figure",
            Description = "Une belle figurine",
            Price = 19.99M,
            Stock = 5
        };

        // Assert
        Assert.True(product.Price > 0);
        Assert.True(product.Price < 100000);
    }

    [Fact]
    public void Product_With_Category_Should_Have_Reference()
    {
        // Arrange
        var category = new Category { Name = "Figurines" };
        var product = new Product 
        { 
            Name = "Figure",
            Description = "Figurine d'horreur",
            Price = 25.00M,
            CategoryId = category.Id
        };

        // Act & Assert
        Assert.NotEqual(Guid.Empty, product.CategoryId);
    }
}