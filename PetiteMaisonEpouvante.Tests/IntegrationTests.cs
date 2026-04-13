using PetiteMaisonEpouvante.API.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        // On configure une "fausse" API pour le test
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // 1. Trouver la configuration SQL Server existante (celle de Program.cs)
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StoreContext>));

                // 2. La supprimer
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // 3. La remplacer par une Base En Mémoire (InMemory)
                services.AddDbContext<StoreContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });
            });
        });
    }

    [Fact]
    public async Task Get_Products_Returns_Success()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.EnsureSuccessStatusCode(); // Vérifie que c'est 200 OK
    }

    [Fact]
    public async Task Post_Product_Without_Token_Returns_Unauthorized()
    {
        // Arrange
        var client = _factory.CreateClient();
        var newItem = new { Name = "Figurine Test", Price = 10.5m, Description = "Test", CategoryId = Guid.NewGuid(), Stock = 5 };
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(newItem), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/products", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode); 
    }
}