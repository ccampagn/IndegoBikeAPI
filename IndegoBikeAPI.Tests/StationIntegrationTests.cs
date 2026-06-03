using System.Net;
using System.Net.Http.Json;
using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IndegoBikeAPI.Tests;

public class StationIntegrationTests : IClassFixture<StationApiFactory>
{
    private readonly HttpClient _client;
    private readonly StationApiFactory _factory;

    public StationIntegrationTests(StationApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SeedStations(params Station[] stations)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IndegoBikeContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        db.Stations.AddRange(stations);
        db.SaveChanges();
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithStationList()
    {
        SeedStations(
            new Station { StationID = 1, StationName = "30th St", IsActive = true },
            new Station { StationID = 2, StationName = "Market St", IsActive = true }
        );

        var response = await _client.GetAsync("/api/stations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stations = await response.Content.ReadFromJsonAsync<List<Station>>();
        Assert.NotNull(stations);
        Assert.Equal(2, stations.Count);
    }

    [Fact]
    public async Task GetAll_EmptyDatabase_ReturnsEmptyArray()
    {
        SeedStations();

        var response = await _client.GetAsync("/api/stations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var stations = await response.Content.ReadFromJsonAsync<List<Station>>();
        Assert.NotNull(stations);
        Assert.Empty(stations);
    }

    [Fact]
    public async Task GetById_ExistingStation_ReturnsCorrectStation()
    {
        SeedStations(new Station
        {
            StationID = 42,
            StationName = "City Hall",
            Latitude = 39.952m,
            Longitude = -75.163m,
            IsActive = true,
            GoLiveDate = new DateOnly(2015, 4, 22)
        });

        var response = await _client.GetAsync("/api/stations/42");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var station = await response.Content.ReadFromJsonAsync<Station>();
        Assert.NotNull(station);
        Assert.Equal(42, station.StationID);
        Assert.Equal("City Hall", station.StationName);
        Assert.Equal(39.952m, station.Latitude);
        Assert.Equal(-75.163m, station.Longitude);
        Assert.Equal(true, station.IsActive);
    }

    [Fact]
    public async Task GetById_NonExistentStation_ReturnsNotFound()
    {
        SeedStations();

        var response = await _client.GetAsync("/api/stations/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetById_InvalidIdFormat_ReturnsNotFound()
    {
        // The {id:int} route constraint means non-integer segments don't match the route at all.
        var response = await _client.GetAsync("/api/stations/not-a-number");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
