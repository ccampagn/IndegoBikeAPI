using System.Net;
using System.Net.Http.Json;
using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.Extensions.DependencyInjection;

namespace IndegoBikeAPI.Tests;

public class RidershipIntegrationTests : IClassFixture<RidershipApiFactory>
{
    private readonly HttpClient _client;
    private readonly RidershipApiFactory _factory;

    public RidershipIntegrationTests(RidershipApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private void SeedData(IEnumerable<Trip> trips, IEnumerable<Bike>? bikes = null)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IndegoBikeContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        if (bikes is not null) db.Bikes.AddRange(bikes);
        db.Trips.AddRange(trips);
        db.SaveChanges();
    }

    private static Trip T(long id, DateTime start,
        int? startStation = null, int? endStation = null,
        int? bikeId = null, int? passTypeId = null) => new()
    {
        TripID = id,
        StartDate = start,
        EndDate = start.AddHours(1),
        StartStationID = startStation,
        EndStationID = endStation,
        BikeID = bikeId,
        PassPlanID = passTypeId
    };

    // ---------------------------------------------------------------
    // GET /api/ridership/by-month
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetByMonth_ReturnsOkWithGroupedData()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 5)),
            T(2, new DateTime(2024, 1, 20)),
            T(3, new DateTime(2024, 3, 15))
        });

        var response = await _client.GetAsync("/api/ridership/by-month");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByMonthDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.First(r => r.Month == 1).TripCount);
        Assert.Equal(1, result.First(r => r.Month == 3).TripCount);
    }

    [Fact]
    public async Task GetByMonth_EmptyDatabase_ReturnsEmptyArray()
    {
        SeedData(Array.Empty<Trip>());

        var response = await _client.GetAsync("/api/ridership/by-month");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByMonthDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByMonth_WithYearFilter_ExcludesOtherYears()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2023, 6, 1)),
            T(2, new DateTime(2024, 6, 1)),
            T(3, new DateTime(2024, 6, 15))
        });

        var response = await _client.GetAsync("/api/ridership/by-month?year=2024");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByMonthDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetByMonth_WithStationFilter_MatchesStartOrEnd()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 4, 1), startStation: 10),
            T(2, new DateTime(2024, 4, 2), endStation: 10),
            T(3, new DateTime(2024, 4, 3), startStation: 99)
        });

        var response = await _client.GetAsync("/api/ridership/by-month?stationId=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByMonthDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GET /api/ridership/by-hour
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetByHour_ReturnsOkWithGroupedData()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1, 8, 0, 0)),
            T(2, new DateTime(2024, 1, 2, 8, 0, 0)),
            T(3, new DateTime(2024, 1, 3, 17, 0, 0))
        });

        var response = await _client.GetAsync("/api/ridership/by-hour");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByHourDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(8, result[0].Hour);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetByHour_WithHourFilter_ReturnsSingleGroup()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1, 8, 0, 0)),
            T(2, new DateTime(2024, 1, 2, 17, 0, 0))
        });

        var response = await _client.GetAsync("/api/ridership/by-hour?hour=8");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByHourDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(8, result[0].Hour);
    }

    // ---------------------------------------------------------------
    // GET /api/ridership/by-station
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetByStation_ReturnsSortedByCountDesc()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1), startStation: 10),
            T(2, new DateTime(2024, 1, 2), startStation: 10),
            T(3, new DateTime(2024, 1, 3), startStation: 20)
        });

        var response = await _client.GetAsync("/api/ridership/by-station");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByStationDto>>();
        Assert.NotNull(result);
        Assert.Equal(10, result[0].StationID);
        Assert.Equal(2, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GET /api/ridership/by-bike
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetByBike_ReturnsSortedByCountDesc()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1), bikeId: 100),
            T(2, new DateTime(2024, 1, 2), bikeId: 100),
            T(3, new DateTime(2024, 1, 3), bikeId: 200)
        });

        var response = await _client.GetAsync("/api/ridership/by-bike");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByBikeDto>>();
        Assert.NotNull(result);
        Assert.Equal(100, result[0].BikeID);
        Assert.Equal(2, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GET /api/ridership/by-bike-type
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetByBikeType_ReturnsGroupedByBikeType()
    {
        SeedData(
            trips: new[]
            {
                T(1, new DateTime(2024, 1, 1), bikeId: 1),
                T(2, new DateTime(2024, 1, 2), bikeId: 1),
                T(3, new DateTime(2024, 1, 3), bikeId: 2)
            },
            bikes: new[]
            {
                new Bike { BikeID = 1, BikeTypeID = 1 },
                new Bike { BikeID = 2, BikeTypeID = 2 }
            });

        var response = await _client.GetAsync("/api/ridership/by-bike-type");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByBikeTypeDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(2, result.First(r => r.BikeTypeID == 1).TripCount);
        Assert.Equal(1, result.First(r => r.BikeTypeID == 2).TripCount);
    }

    // ---------------------------------------------------------------
    // GET /api/ridership/top-routes
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetTopRoutes_ReturnsSortedByCountDesc()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1), startStation: 10, endStation: 20),
            T(2, new DateTime(2024, 1, 2), startStation: 10, endStation: 20),
            T(3, new DateTime(2024, 1, 3), startStation: 30, endStation: 40)
        });

        var response = await _client.GetAsync("/api/ridership/top-routes");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<TopRoutePairingDto>>();
        Assert.NotNull(result);
        Assert.Equal(10, result[0].StartStationID);
        Assert.Equal(20, result[0].EndStationID);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetTopRoutes_WithStationFilter_OnlyMatchingRoutes()
    {
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1), startStation: 10, endStation: 20),
            T(2, new DateTime(2024, 1, 2), startStation: 10, endStation: 20),
            T(3, new DateTime(2024, 1, 3), startStation: 99, endStation: 88)
        });

        var response = await _client.GetAsync("/api/ridership/top-routes?stationId=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<TopRoutePairingDto>>();
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GET /api/ridership/by-day-of-week
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetByDayOfWeek_ReturnsOkWithGroupedData()
    {
        // 2024-01-01 = Monday (1), 2024-01-02 = Tuesday (2)
        SeedData(new[]
        {
            T(1, new DateTime(2024, 1, 1)),
            T(2, new DateTime(2024, 1, 1)),
            T(3, new DateTime(2024, 1, 2))
        });

        var response = await _client.GetAsync("/api/ridership/by-day-of-week");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByDayOfWeekDto>>();
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].DayOfWeek);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetByDayOfWeek_EmptyDatabase_ReturnsEmptyArray()
    {
        SeedData(Array.Empty<Trip>());

        var response = await _client.GetAsync("/api/ridership/by-day-of-week");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<RidershipByDayOfWeekDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
