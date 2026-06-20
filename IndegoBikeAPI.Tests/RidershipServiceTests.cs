using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using IndegoBikeAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Tests;

public class RidershipServiceTests
{
    private static IndegoBikeContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<IndegoBikeContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new IndegoBikeContext(options);
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
    // GetRidershipByMonthAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetRidershipByMonth_GroupsAndOrdersByMonth()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_GroupsAndOrdersByMonth));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 5)),
            T(2, new DateTime(2024, 1, 20)),
            T(3, new DateTime(2024, 3, 10)));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].Month); Assert.Equal(2, result[0].TripCount);
        Assert.Equal(3, result[1].Month); Assert.Equal(1, result[1].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_NoTrips_ReturnsEmpty()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_NoTrips_ReturnsEmpty));

        var result = await new RidershipService(db).GetRidershipByMonthAsync(new());

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByStationId_MatchesStartOrEnd()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByStationId_MatchesStartOrEnd));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 5, 1), startStation: 10),
            T(2, new DateTime(2024, 5, 2), endStation: 10),
            T(3, new DateTime(2024, 5, 3), startStation: 99));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { StationId = 10 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByYear_ExcludesOtherYears()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByYear_ExcludesOtherYears));
        db.Trips.AddRange(
            T(1, new DateTime(2023, 6, 1)),
            T(2, new DateTime(2024, 6, 1)),
            T(3, new DateTime(2024, 6, 15)));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { Year = 2024 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByMonth_OnlyMatchingMonth()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByMonth_OnlyMatchingMonth));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 2, 10)),
            T(2, new DateTime(2024, 3, 10)),
            T(3, new DateTime(2024, 3, 20)));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { Month = 3 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByHour()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByHour));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 5, 1, 8, 0, 0)),
            T(2, new DateTime(2024, 5, 2, 8, 0, 0)),
            T(3, new DateTime(2024, 5, 3, 17, 0, 0)));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { Hour = 8 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByBikeId()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByBikeId));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 4, 1), bikeId: 5),
            T(2, new DateTime(2024, 4, 2), bikeId: 5),
            T(3, new DateTime(2024, 4, 3), bikeId: 9));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { BikeId = 5 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByPassTypeId()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByPassTypeId));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 4, 1), passTypeId: 2),
            T(2, new DateTime(2024, 4, 2), passTypeId: 2),
            T(3, new DateTime(2024, 4, 3), passTypeId: 7));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { PassTypeId = 2 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByMonth_FilterByBikeTypeId()
    {
        using var db = CreateContext(nameof(GetRidershipByMonth_FilterByBikeTypeId));
        db.Bikes.AddRange(
            new Bike { BikeID = 1, BikeTypeID = 1 },
            new Bike { BikeID = 2, BikeTypeID = 2 });
        db.Trips.AddRange(
            T(1, new DateTime(2024, 6, 1), bikeId: 1),
            T(2, new DateTime(2024, 6, 2), bikeId: 1),
            T(3, new DateTime(2024, 6, 3), bikeId: 2));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByMonthAsync(new() { BikeTypeId = 1 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GetRidershipByHourAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetRidershipByHour_GroupsAndOrdersByHour()
    {
        using var db = CreateContext(nameof(GetRidershipByHour_GroupsAndOrdersByHour));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1, 8, 0, 0)),
            T(2, new DateTime(2024, 1, 2, 8, 30, 0)),
            T(3, new DateTime(2024, 1, 3, 17, 0, 0)));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByHourAsync(new())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(8, result[0].Hour);  Assert.Equal(2, result[0].TripCount);
        Assert.Equal(17, result[1].Hour); Assert.Equal(1, result[1].TripCount);
    }

    [Fact]
    public async Task GetRidershipByHour_FilterByStationId()
    {
        using var db = CreateContext(nameof(GetRidershipByHour_FilterByStationId));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1, 8, 0, 0), startStation: 10),
            T(2, new DateTime(2024, 1, 2, 9, 0, 0), startStation: 99));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByHourAsync(new() { StationId = 10 })).ToList();

        Assert.Single(result);
        Assert.Equal(8, result[0].Hour);
    }

    // ---------------------------------------------------------------
    // GetRidershipByStationAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetRidershipByStation_GroupsByStartStation_OrderedByCountDesc()
    {
        using var db = CreateContext(nameof(GetRidershipByStation_GroupsByStartStation_OrderedByCountDesc));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1), startStation: 10),
            T(2, new DateTime(2024, 1, 2), startStation: 10),
            T(3, new DateTime(2024, 1, 3), startStation: 20));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByStationAsync(new())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(10, result[0].StationID); Assert.Equal(2, result[0].TripCount);
        Assert.Equal(20, result[1].StationID); Assert.Equal(1, result[1].TripCount);
    }

    [Fact]
    public async Task GetRidershipByStation_FilterByYear()
    {
        using var db = CreateContext(nameof(GetRidershipByStation_FilterByYear));
        db.Trips.AddRange(
            T(1, new DateTime(2023, 1, 1), startStation: 10),
            T(2, new DateTime(2024, 1, 1), startStation: 10),
            T(3, new DateTime(2024, 1, 2), startStation: 20));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByStationAsync(new() { Year = 2024 })).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result.First(r => r.StationID == 10).TripCount);
    }

    // ---------------------------------------------------------------
    // GetRidershipByBikeAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetRidershipByBike_GroupsByBike_OrderedByCountDesc()
    {
        using var db = CreateContext(nameof(GetRidershipByBike_GroupsByBike_OrderedByCountDesc));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1), bikeId: 100),
            T(2, new DateTime(2024, 1, 2), bikeId: 100),
            T(3, new DateTime(2024, 1, 3), bikeId: 200));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByBikeAsync(new())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(100, result[0].BikeID); Assert.Equal(2, result[0].TripCount);
        Assert.Equal(200, result[1].BikeID); Assert.Equal(1, result[1].TripCount);
    }

    // ---------------------------------------------------------------
    // GetRidershipByBikeTypeAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetRidershipByBikeType_JoinsAndGroupsByBikeType()
    {
        using var db = CreateContext(nameof(GetRidershipByBikeType_JoinsAndGroupsByBikeType));
        db.Bikes.AddRange(
            new Bike { BikeID = 1, BikeTypeID = 1 },
            new Bike { BikeID = 2, BikeTypeID = 1 },
            new Bike { BikeID = 3, BikeTypeID = 2 });
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1), bikeId: 1),
            T(2, new DateTime(2024, 1, 2), bikeId: 2),
            T(3, new DateTime(2024, 1, 3), bikeId: 3));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByBikeTypeAsync(new()))
            .OrderBy(r => r.BikeTypeID).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].BikeTypeID); Assert.Equal(2, result[0].TripCount);
        Assert.Equal(2, result[1].BikeTypeID); Assert.Equal(1, result[1].TripCount);
    }

    [Fact]
    public async Task GetRidershipByBikeType_TripsWithNoBikeRecord_Excluded()
    {
        using var db = CreateContext(nameof(GetRidershipByBikeType_TripsWithNoBikeRecord_Excluded));
        db.Bikes.Add(new Bike { BikeID = 1, BikeTypeID = 1 });
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1), bikeId: 1),
            T(2, new DateTime(2024, 1, 2), bikeId: 999)); // no matching Bike row
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByBikeTypeAsync(new())).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GetTopRoutePairingsAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetTopRoutePairings_ReturnsSortedByCountDesc()
    {
        using var db = CreateContext(nameof(GetTopRoutePairings_ReturnsSortedByCountDesc));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1), startStation: 10, endStation: 20),
            T(2, new DateTime(2024, 1, 2), startStation: 10, endStation: 20),
            T(3, new DateTime(2024, 1, 3), startStation: 30, endStation: 40));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetTopRoutePairingsAsync(new())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(10, result[0].StartStationID);
        Assert.Equal(20, result[0].EndStationID);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetTopRoutePairings_LimitedTo50()
    {
        using var db = CreateContext(nameof(GetTopRoutePairings_LimitedTo50));
        db.Trips.AddRange(Enumerable.Range(1, 60)
            .Select(i => T(i, new DateTime(2024, 1, 1), startStation: i, endStation: i + 100)));
        await db.SaveChangesAsync();

        var result = await new RidershipService(db).GetTopRoutePairingsAsync(new());

        Assert.Equal(50, result.Count());
    }

    [Fact]
    public async Task GetTopRoutePairings_FilterByStationId()
    {
        using var db = CreateContext(nameof(GetTopRoutePairings_FilterByStationId));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1), startStation: 10, endStation: 20),
            T(2, new DateTime(2024, 1, 2), startStation: 10, endStation: 20),
            T(3, new DateTime(2024, 1, 3), startStation: 99, endStation: 88));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetTopRoutePairingsAsync(new() { StationId = 10 })).ToList();

        Assert.Single(result);
        Assert.Equal(2, result[0].TripCount);
    }

    // ---------------------------------------------------------------
    // GetRidershipByDayOfWeekAsync
    // ---------------------------------------------------------------

    [Fact]
    public async Task GetRidershipByDayOfWeek_GroupsAndOrdersByDayOfWeek()
    {
        using var db = CreateContext(nameof(GetRidershipByDayOfWeek_GroupsAndOrdersByDayOfWeek));
        // 2024-01-01 = Monday (1), 2024-01-02 = Tuesday (2)
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1)),
            T(2, new DateTime(2024, 1, 1)),
            T(3, new DateTime(2024, 1, 2)));
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByDayOfWeekAsync(new())).ToList();

        Assert.Equal(2, result.Count);
        Assert.Equal(1, result[0].DayOfWeek); Assert.Equal(2, result[0].TripCount);
        Assert.Equal(2, result[1].DayOfWeek); Assert.Equal(1, result[1].TripCount);
    }

    [Fact]
    public async Task GetRidershipByDayOfWeek_FilterByDayOfWeek()
    {
        using var db = CreateContext(nameof(GetRidershipByDayOfWeek_FilterByDayOfWeek));
        db.Trips.AddRange(
            T(1, new DateTime(2024, 1, 1)), // Monday (1)
            T(2, new DateTime(2024, 1, 1)), // Monday (1)
            T(3, new DateTime(2024, 1, 2))); // Tuesday (2)
        await db.SaveChangesAsync();

        var result = (await new RidershipService(db).GetRidershipByDayOfWeekAsync(new() { DayOfWeek = 1 })).ToList();

        Assert.Single(result);
        Assert.Equal(1, result[0].DayOfWeek);
        Assert.Equal(2, result[0].TripCount);
    }

    [Fact]
    public async Task GetRidershipByDayOfWeek_NoTrips_ReturnsEmpty()
    {
        using var db = CreateContext(nameof(GetRidershipByDayOfWeek_NoTrips_ReturnsEmpty));

        var result = await new RidershipService(db).GetRidershipByDayOfWeekAsync(new());

        Assert.Empty(result);
    }
}
