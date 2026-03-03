using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using PetiteMaisonEpouvante.API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

public class LoadTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoadTests(WebApplicationFactory<Program> factory)
    {
        // same setup as integration tests: replace the real database with InMemory
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<StoreContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<StoreContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForLoadTesting");
                });
            });
        });
    }

    [Fact(Timeout = 120_000)]
    public async Task GetItems_LoadSimulation_CustomConcurrency()
    {
        // setup test server client
        var client = _factory.CreateClient();

        int totalRequests = 1000;          // number of requests to send
        int concurrency = 100;            // number of parallel workers

        var semaphore = new System.Threading.SemaphoreSlim(concurrency);
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < totalRequests; i++)
        {
            await semaphore.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    return await client.GetAsync("/api/items");
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        var responses = await Task.WhenAll(tasks);
        // ensure at least one successful response (sanity check)
        Assert.Contains(responses, r => r.IsSuccessStatusCode);
    }
}