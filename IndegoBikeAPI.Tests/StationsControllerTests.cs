using IndegoBikeAPI.Controllers;
using IndegoBikeAPI.Models;
using IndegoBikeAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace IndegoBikeAPI.Tests;

public class StationsControllerTests
{
    [Fact]
    public async Task GetAll_ReturnsOkWithStations()
    {
        var stations = new List<Station>
        {
            new() { StationID = 1, StationName = "30th St" },
            new() { StationID = 2, StationName = "Market St" }
        };
        var mock = new Mock<IStationService>();
        mock.Setup(s => s.GetAllAsync()).ReturnsAsync(stations);

        var controller = new StationsController(mock.Object);
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsAssignableFrom<IEnumerable<Station>>(ok.Value);
        Assert.Equal(2, body.Count());
    }

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkWithStation()
    {
        var station = new Station { StationID = 5, StationName = "Broad St" };
        var mock = new Mock<IStationService>();
        mock.Setup(s => s.GetByIdAsync(5)).ReturnsAsync(station);

        var controller = new StationsController(mock.Object);
        var result = await controller.GetById(5);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var body = Assert.IsType<Station>(ok.Value);
        Assert.Equal("Broad St", body.StationName);
    }

    [Fact]
    public async Task GetById_MissingId_ReturnsNotFound()
    {
        var mock = new Mock<IStationService>();
        mock.Setup(s => s.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((Station?)null);
        var controller = new StationsController(mock.Object);
        var result = await controller.GetById(999);
        Assert.IsType<NotFoundResult>(result.Result);
    }
}
