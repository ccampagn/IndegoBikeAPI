using IndegoBikeAPI.Models;

namespace IndegoBikeAPI.Tests;

// ---------------------------------------------------------------
// RidershipByMonthDto
// ---------------------------------------------------------------
public class RidershipByMonthDtoTests
{
    [Fact]
    public void DefaultValues_AreZero()
    {
        var dto = new RidershipByMonthDto();

        Assert.Equal(0, dto.Month);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new RidershipByMonthDto { Month = 6, TripCount = 1234 };

        Assert.Equal(6,    dto.Month);
        Assert.Equal(1234, dto.TripCount);
    }
}

// ---------------------------------------------------------------
// RidershipByDayOfWeekDto
// ---------------------------------------------------------------
public class RidershipByDayOfWeekDtoTests
{
    [Fact]
    public void DefaultValues_AreZero()
    {
        var dto = new RidershipByDayOfWeekDto();

        Assert.Equal(0, dto.DayOfWeek);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new RidershipByDayOfWeekDto { DayOfWeek = 1, TripCount = 500 };

        Assert.Equal(1,   dto.DayOfWeek);
        Assert.Equal(500, dto.TripCount);
    }

    [Fact]
    public void DayOfWeek_AcceptsFullRange_ZeroToSix()
    {
        foreach (var day in Enumerable.Range(0, 7))
        {
            var dto = new RidershipByDayOfWeekDto { DayOfWeek = day };
            Assert.Equal(day, dto.DayOfWeek);
        }
    }
}

// ---------------------------------------------------------------
// RidershipByHourDto
// ---------------------------------------------------------------
public class RidershipByHourDtoTests
{
    [Fact]
    public void DefaultValues_AreZero()
    {
        var dto = new RidershipByHourDto();

        Assert.Equal(0, dto.Hour);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new RidershipByHourDto { Hour = 17, TripCount = 320 };

        Assert.Equal(17,  dto.Hour);
        Assert.Equal(320, dto.TripCount);
    }

    [Fact]
    public void Hour_AcceptsFullRange_ZeroTo23()
    {
        foreach (var hour in Enumerable.Range(0, 24))
        {
            var dto = new RidershipByHourDto { Hour = hour };
            Assert.Equal(hour, dto.Hour);
        }
    }
}

// ---------------------------------------------------------------
// RidershipByStationDto
// ---------------------------------------------------------------
public class RidershipByStationDtoTests
{
    [Fact]
    public void DefaultValues_StationIDIsNull_TripCountIsZero()
    {
        var dto = new RidershipByStationDto();

        Assert.Null(dto.StationID);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new RidershipByStationDto { StationID = 3006, TripCount = 8500 };

        Assert.Equal(3006, dto.StationID);
        Assert.Equal(8500, dto.TripCount);
    }

    [Fact]
    public void StationID_CanBeNull()
    {
        var dto = new RidershipByStationDto { StationID = null };

        Assert.Null(dto.StationID);
    }
}

// ---------------------------------------------------------------
// RidershipByBikeDto
// ---------------------------------------------------------------
public class RidershipByBikeDtoTests
{
    [Fact]
    public void DefaultValues_BikeIDIsNull_TripCountIsZero()
    {
        var dto = new RidershipByBikeDto();

        Assert.Null(dto.BikeID);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new RidershipByBikeDto { BikeID = 42, TripCount = 275 };

        Assert.Equal(42,  dto.BikeID);
        Assert.Equal(275, dto.TripCount);
    }

    [Fact]
    public void BikeID_CanBeNull()
    {
        var dto = new RidershipByBikeDto { BikeID = null };

        Assert.Null(dto.BikeID);
    }
}

// ---------------------------------------------------------------
// RidershipByBikeTypeDto
// ---------------------------------------------------------------
public class RidershipByBikeTypeDtoTests
{
    [Fact]
    public void DefaultValues_BikeTypeIDIsNull_TripCountIsZero()
    {
        var dto = new RidershipByBikeTypeDto();

        Assert.Null(dto.BikeTypeID);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new RidershipByBikeTypeDto { BikeTypeID = 2, TripCount = 4500 };

        Assert.Equal(2,    dto.BikeTypeID);
        Assert.Equal(4500, dto.TripCount);
    }

    [Fact]
    public void BikeTypeID_CanBeNull()
    {
        var dto = new RidershipByBikeTypeDto { BikeTypeID = null };

        Assert.Null(dto.BikeTypeID);
    }
}

// ---------------------------------------------------------------
// TopRoutePairingDto
// ---------------------------------------------------------------
public class TopRoutePairingDtoTests
{
    [Fact]
    public void DefaultValues_StationIDsAreNull_TripCountIsZero()
    {
        var dto = new TopRoutePairingDto();

        Assert.Null(dto.StartStationID);
        Assert.Null(dto.EndStationID);
        Assert.Equal(0, dto.TripCount);
    }

    [Fact]
    public void Properties_RoundTrip()
    {
        var dto = new TopRoutePairingDto
        {
            StartStationID = 3006,
            EndStationID   = 3021,
            TripCount      = 1800
        };

        Assert.Equal(3006, dto.StartStationID);
        Assert.Equal(3021, dto.EndStationID);
        Assert.Equal(1800, dto.TripCount);
    }

    [Fact]
    public void StartAndEndStation_CanBeTheSame()
    {
        var dto = new TopRoutePairingDto { StartStationID = 10, EndStationID = 10 };

        Assert.Equal(dto.StartStationID, dto.EndStationID);
    }

    [Fact]
    public void StartAndEndStation_CanBeNull()
    {
        var dto = new TopRoutePairingDto { StartStationID = null, EndStationID = null };

        Assert.Null(dto.StartStationID);
        Assert.Null(dto.EndStationID);
    }
}

// ---------------------------------------------------------------
// TripFilterParams
// ---------------------------------------------------------------
public class TripFilterParamsTests
{
    [Fact]
    public void AllProperties_DefaultToNull()
    {
        var f = new TripFilterParams();

        Assert.Null(f.StationId);
        Assert.Null(f.BikeId);
        Assert.Null(f.BikeTypeId);
        Assert.Null(f.PassTypeId);
        Assert.Null(f.Month);
        Assert.Null(f.Year);
        Assert.Null(f.DayOfWeek);
        Assert.Null(f.Hour);
    }

    [Fact]
    public void AllProperties_CanBeSet()
    {
        var f = new TripFilterParams
        {
            StationId  = 10,
            BikeId     = 42,
            BikeTypeId = 2,
            PassTypeId = 3,
            Month      = 6,
            Year       = 2024,
            DayOfWeek  = 1,
            Hour       = 8
        };

        Assert.Equal(10,   f.StationId);
        Assert.Equal(42,   f.BikeId);
        Assert.Equal(2,    f.BikeTypeId);
        Assert.Equal(3,    f.PassTypeId);
        Assert.Equal(6,    f.Month);
        Assert.Equal(2024, f.Year);
        Assert.Equal(1,    f.DayOfWeek);
        Assert.Equal(8,    f.Hour);
    }

    [Fact]
    public void Properties_CanBeSetToNull_AfterAssignment()
    {
        var f = new TripFilterParams { Year = 2024, Month = 6 };
        f.Year  = null;
        f.Month = null;

        Assert.Null(f.Year);
        Assert.Null(f.Month);
    }

    [Fact]
    public void HasExactlyEightFilterProperties()
    {
        var count = typeof(TripFilterParams).GetProperties().Length;

        Assert.Equal(8, count);
    }
}
