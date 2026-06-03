using IndegoBikeAPI.Controllers;
using IndegoBikeAPI.Models;
using IndegoBikeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndegoBikeAPI.Tests;

public class RidershipControllerTests
{
    private static TripFilterParams Empty => new();

    [Fact]
    public async Task GetByMonth_ReturnsOkWithServiceResult()
    {
        var data = new List<RidershipByMonthDto> { new() { Month = 6, TripCount = 120 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByMonthAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetByMonth(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetByDayOfWeek_ReturnsOkWithServiceResult()
    {
        var data = new List<RidershipByDayOfWeekDto> { new() { DayOfWeek = 1, TripCount = 80 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByDayOfWeekAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetByDayOfWeek(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetByHour_ReturnsOkWithServiceResult()
    {
        var data = new List<RidershipByHourDto> { new() { Hour = 8, TripCount = 300 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByHourAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetByHour(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetByStation_ReturnsOkWithServiceResult()
    {
        var data = new List<RidershipByStationDto> { new() { StationID = 3006, TripCount = 500 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByStationAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetByStation(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetByBike_ReturnsOkWithServiceResult()
    {
        var data = new List<RidershipByBikeDto> { new() { BikeID = 42, TripCount = 150 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByBikeAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetByBike(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetByBikeType_ReturnsOkWithServiceResult()
    {
        var data = new List<RidershipByBikeTypeDto> { new() { BikeTypeID = 1, TripCount = 1200 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByBikeTypeAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetByBikeType(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetTopRoutes_ReturnsOkWithServiceResult()
    {
        var data = new List<TopRoutePairingDto> { new() { StartStationID = 10, EndStationID = 20, TripCount = 400 } };
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetTopRoutePairingsAsync(It.IsAny<TripFilterParams>())).ReturnsAsync(data);

        var result = await new RidershipController(mock.Object).GetTopRoutes(Empty);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(data, ok.Value);
    }

    [Fact]
    public async Task GetByMonth_PassesFiltersToService()
    {
        var mock = new Mock<IRidershipService>();
        mock.Setup(s => s.GetRidershipByMonthAsync(It.IsAny<TripFilterParams>()))
            .ReturnsAsync(new List<RidershipByMonthDto>());

        var filters = new TripFilterParams { Year = 2024, StationId = 10 };
        await new RidershipController(mock.Object).GetByMonth(filters);

        mock.Verify(s => s.GetRidershipByMonthAsync(
            It.Is<TripFilterParams>(f => f.Year == 2024 && f.StationId == 10)), Times.Once);
    }
}
