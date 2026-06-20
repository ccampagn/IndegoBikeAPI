 using IndegoBikeAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IndegoBikeAPI.Tests;

public class RidershipApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<IndegoBikeContext>) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<IndegoBikeContext>))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<IndegoBikeContext>(options =>
                options.UseInMemoryDatabase("RidershipIntegrationTestDb"));
        });

        builder.UseEnvironment("Development");
    }
}
