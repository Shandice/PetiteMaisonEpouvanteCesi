using PetiteMaisonEpouvante.Core;

namespace PetiteMaisonEpouvante.Tests;

public class ExchangePostTests
{
    [Fact]
    public void ExchangePost_Should_Initialize_As_Active()
    {
        // Arrange & Act
        var post = new ExchangePost 
        { 
            UserId = "user123",
            UserName = "DocMallory",
            Title = "Échange BD",
            Description = "Je cherche l'orc tome 2",
            Type = ExchangeType.Exchange
        };

        // Assert
        Assert.True(post.IsActive);
        Assert.Equal(ExchangeType.Exchange, post.Type);
    }

    [Fact]
    public void ExchangePost_Should_Support_Give_Type()
    {
        // Arrange & Act
        var post = new ExchangePost 
        { 
            UserId = "user456",
            UserName = "Alice",
            Title = "Donner DVD",
            Type = ExchangeType.Give,
            IsActive = true
        };

        // Assert
        Assert.Equal(ExchangeType.Give, post.Type);
    }

    [Fact]
    public void ExchangePost_Should_Support_Wish_Type()
    {
        // Arrange & Act
        var post = new ExchangePost 
        { 
            Type = ExchangeType.Wish,
            Title = "Cherche Blu-ray Nosferatu"
        };

        // Assert
        Assert.Equal(ExchangeType.Wish, post.Type);
    }

    [Fact]
    public void ExchangePost_Should_Can_Be_Deactivated()
    {
        // Arrange
        var post = new ExchangePost { IsActive = true };

        // Act
        post.IsActive = false;

        // Assert
        Assert.False(post.IsActive);
    }

    [Fact]
    public void ExchangePost_Should_Support_Tags()
    {
        // Arrange & Act
        var post = new ExchangePost 
        { 
            Tags = new List<string> { "horror", "bd", "french" }
        };

        // Assert
        Assert.Contains("horror", post.Tags);
        Assert.Equal(3, post.Tags.Count);
    }
}
