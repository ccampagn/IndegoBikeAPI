using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using IndegoBikeAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Tests;

public class StationServiceTests
{
    private static IndegoBikeContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<IndegoBikeContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new IndegoBikeContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllStations()
    {
        using var db = CreateContext(nameof(GetAllAsync_ReturnsAllStations));
        db.Stations.AddRange(
            new Station { StationID = 1, StationName = "30th St", IsActive = true },
            new Station { StationID = 2, StationName = "Market St", IsActive = false }
        );
        await db.SaveChangesAsync();

        var service = new StationService(db);
        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_EmptyDatabase_ReturnsEmpty()
    {
        using var db = CreateContext(nameof(GetAllAsync_EmptyDatabase_ReturnsEmpty));

        var service = new StationService(db);
        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsStation()
    {
        using var db = CreateContext(nameof(GetByIdAsync_ExistingId_ReturnsStation));
        db.Stations.Add(new Station
        {
            StationID = 10,
            StationName = "City Hall",
            Latitude = 39.952m,
            Longitude = -75.163m,
            IsActive = true,
            GoLiveDate = new DateOnly(2015, 4, 22)
        });
        await db.SaveChangesAsync();

        var service = new StationService(db);
        var station = await service.GetByIdAsync(10);

        Assert.NotNull(station);
        Assert.Equal("City Hall", station.StationName);
        Assert.Equal(39.952m, station.Latitude);
    }

    [Fact]
    public async Task GetByIdAsync_MissingId_ReturnsNull()
    {
        using var db = CreateContext(nameof(GetByIdAsync_MissingId_ReturnsNull));

        var service = new StationService(db);
        var station = await service.GetByIdAsync(999);

        Assert.Null(station);
    }
}
