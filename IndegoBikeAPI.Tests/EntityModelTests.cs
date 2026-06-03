using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IndegoBikeAPI.Models;

namespace IndegoBikeAPI.Tests;

// ---------------------------------------------------------------
// Station
// ---------------------------------------------------------------
public class StationModelTests
{
    [Fact]
    public void Station_HasTableAttribute_WithCorrectName()
    {
        var attr = typeof(Station).GetCustomAttributes(typeof(TableAttribute), false)
            .Cast<TableAttribute>().Single();

        Assert.Equal("Station", attr.Name);
    }

    [Fact]
    public void StationID_HasKeyAttribute()
    {
        var prop = typeof(Station).GetProperty(nameof(Station.StationID))!;

        Assert.True(prop.GetCustomAttributes(typeof(KeyAttribute), false).Any());
    }

    [Fact]
    public void NewStation_NullableProperties_DefaultToNull()
    {
        var s = new Station();

        Assert.Null(s.StationName);
        Assert.Null(s.GoLiveDate);
        Assert.Null(s.Latitude);
        Assert.Null(s.Longitude);
        Assert.Null(s.IsActive);
    }

    [Fact]
    public void Station_Properties_RoundTrip()
    {
        var goLive = new DateOnly(2015, 4, 22);
        var s = new Station
        {
            StationID   = 42,
            StationName = "City Hall",
            GoLiveDate  = goLive,
            Latitude    = 39.9526m,
            Longitude   = -75.1652m,
            IsActive    = true
        };

        Assert.Equal(42,            s.StationID);
        Assert.Equal("City Hall",   s.StationName);
        Assert.Equal(goLive,        s.GoLiveDate);
        Assert.Equal(39.9526m,      s.Latitude);
        Assert.Equal(-75.1652m,     s.Longitude);
        Assert.True(s.IsActive);
    }

    [Fact]
    public void Station_IsActive_CanBeFalse()
    {
        var s = new Station { IsActive = false };

        Assert.False(s.IsActive);
    }
}

// ---------------------------------------------------------------
// Trip
// ---------------------------------------------------------------
public class TripModelTests
{
    [Fact]
    public void Trip_HasTableAttribute_WithCorrectName()
    {
        var attr = typeof(Trip).GetCustomAttributes(typeof(TableAttribute), false)
            .Cast<TableAttribute>().Single();

        Assert.Equal("Trip", attr.Name);
    }

    [Fact]
    public void TripID_HasKeyAttribute()
    {
        var prop = typeof(Trip).GetProperty(nameof(Trip.TripID))!;

        Assert.True(prop.GetCustomAttributes(typeof(KeyAttribute), false).Any());
    }

    [Fact]
    public void NewTrip_NullableProperties_DefaultToNull()
    {
        var t = new Trip();

        Assert.Null(t.StartStationID);
        Assert.Null(t.EndStationID);
        Assert.Null(t.BikeID);
        Assert.Null(t.PassPlanID);
    }

    [Fact]
    public void NewTrip_TripID_IsLongType()
    {
        Assert.Equal(typeof(long), typeof(Trip).GetProperty(nameof(Trip.TripID))!.PropertyType);
    }

    [Fact]
    public void Trip_Properties_RoundTrip()
    {
        var start = new DateTime(2024, 6, 15, 8, 30, 0);
        var end   = new DateTime(2024, 6, 15, 9, 15, 0);

        var t = new Trip
        {
            TripID         = 100_000L,
            StartDate      = start,
            EndDate        = end,
            StartStationID = 3006,
            EndStationID   = 3021,
            BikeID         = 42,
            PassPlanID     = 2
        };

        Assert.Equal(100_000L, t.TripID);
        Assert.Equal(start,    t.StartDate);
        Assert.Equal(end,      t.EndDate);
        Assert.Equal(3006,     t.StartStationID);
        Assert.Equal(3021,     t.EndStationID);
        Assert.Equal(42,       t.BikeID);
        Assert.Equal(2,        t.PassPlanID);
    }

    [Fact]
    public void Trip_StartAndEndStation_CanBeIndependent()
    {
        var t = new Trip { StartStationID = 10, EndStationID = 20 };

        Assert.NotEqual(t.StartStationID, t.EndStationID);
    }
}

// ---------------------------------------------------------------
// Bike
// ---------------------------------------------------------------
public class BikeModelTests
{
    [Fact]
    public void Bike_HasTableAttribute_WithCorrectName()
    {
        var attr = typeof(Bike).GetCustomAttributes(typeof(TableAttribute), false)
            .Cast<TableAttribute>().Single();

        Assert.Equal("Bike", attr.Name);
    }

    [Fact]
    public void BikeID_HasKeyAttribute()
    {
        var prop = typeof(Bike).GetProperty(nameof(Bike.BikeID))!;

        Assert.True(prop.GetCustomAttributes(typeof(KeyAttribute), false).Any());
    }

    [Fact]
    public void NewBike_BikeTypeID_DefaultsToNull()
    {
        var b = new Bike();

        Assert.Null(b.BikeTypeID);
    }

    [Fact]
    public void Bike_Properties_RoundTrip()
    {
        var b = new Bike { BikeID = 7, BikeTypeID = 2 };

        Assert.Equal(7, b.BikeID);
        Assert.Equal(2, b.BikeTypeID);
    }
}
