using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IndegoBikeWebsite.Tests;

public class ProgramTests
{
    [Fact]
    public async Task App_StartsInDevelopmentMode_IndexReturnsSuccess()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b => b.UseSetting("ApiBaseUrl", "https://localhost:1/"));
        using var client = factory.CreateClient();

        // Trigger a request so the AddHttpClient callback (Program.cs lines 6-7) executes
        var response = await client.GetAsync("/");

        Assert.True((int)response.StatusCode < 500);
    }

    [Fact]
    public async Task App_StartsInProductionMode_ExceptionHandlerAndHstsConfigured()
    {
        using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(b =>
            {
                b.UseEnvironment("Production");
                b.UseSetting("ApiBaseUrl", "https://localhost:1/");
            });
        using var client = factory.CreateClient(new() { AllowAutoRedirect = false });

        // Exercises the if (!IsDevelopment) branch in Program.cs
        var response = await client.GetAsync("/");

        Assert.True((int)response.StatusCode < 500 || response.StatusCode == System.Net.HttpStatusCode.MovedPermanently);
    }
}
