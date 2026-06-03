using IndegoBikeAPI.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace IndegoBikeAPI.Tests;

public class StationApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // EF Core 9 stores the provider config in IDbContextOptionsConfiguration<T>;
            // removing only DbContextOptions<T> leaves the SQL Server config in place.
            var toRemove = services
                .Where(d =>
                    d.ServiceType == typeof(DbContextOptions<IndegoBikeContext>) ||
                    d.ServiceType == typeof(IDbContextOptionsConfiguration<IndegoBikeContext>))
                .ToList();

            foreach (var d in toRemove)
                services.Remove(d);

            services.AddDbContext<IndegoBikeContext>(options =>
                options.UseInMemoryDatabase("IntegrationTestDb"));
        });

        builder.UseEnvironment("Development");
    }
}
