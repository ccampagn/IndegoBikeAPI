using System.Diagnostics;
using System.Net;
using IndegoBikeWebsite.Controllers;
using IndegoBikeWebsite.Models;
using IndegoBikeWebsite.Tests.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace IndegoBikeWebsite.Tests;

public class HomeControllerTests
{
    private static readonly List<StationDto> DefaultStations =
    [
        new(1, "30th St"),
        new(2, "Market St")
    ];

    private static (HomeController controller, FakeHttpMessageHandler handler) Build(
        Action<FakeHttpMessageHandler>? configure = null)
    {
        var handler = new FakeHttpMessageHandler();
        handler.Setup("api/stations", HttpStatusCode.OK, DefaultStations);
        configure?.Invoke(handler);

        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("IndegoBikeAPI")).Returns(client);

        var controller = new HomeController(NullLogger<HomeController>.Instance, factory.Object);
        return (controller, handler);
    }

    // Register "by-bike-type" before "by-bike" so the more-specific path matches first.
    private static void SetupAllRidership(FakeHttpMessageHandler h)
    {
        h.Setup("by-month",       HttpStatusCode.OK, new List<ByMonthRow>   { new(6, 500) });
        h.Setup("by-day-of-week", HttpStatusCode.OK, new List<ByDayRow>     { new(1, 300) });
        h.Setup("by-hour",        HttpStatusCode.OK, new List<ByHourRow>    { new(8, 400) });
        h.Setup("by-station",     HttpStatusCode.OK, new List<ByStationRow> { new(1, 200) });
        h.Setup("by-bike-type",   HttpStatusCode.OK, new List<ByBikeTypeRow>{ new(1, 600) });
        h.Setup("by-bike",        HttpStatusCode.OK, new List<ByBikeRow>    { new(42, 50) });
        h.Setup("top-routes",     HttpStatusCode.OK, new List<TopRouteRow>  { new(1, 2, 100) });
    }

    private static RidershipViewModel GetViewModel(IActionResult result)
    {
        var view = Assert.IsType<ViewResult>(result);
        return Assert.IsType<RidershipViewModel>(view.Model);
    }

    [Fact]
    public async Task Index_NotSubmitted_LoadsStationsFromApi()
    {
        var (controller, _) = Build();

        var result = await controller.Index(null, null, null, null, null, null, submitted: false);

        var vm = GetViewModel(result);
        Assert.Equal(2, vm.Stations.Count);
        Assert.Equal("30th St", vm.Stations[0].StationName);
        Assert.Equal("Market St", vm.Stations[1].StationName);
    }

    [Fact]
    public async Task Index_NotSubmitted_ResultsNotLoaded()
    {
        var (controller, _) = Build();

        var result = await controller.Index(null, null, null, null, null, null, submitted: false);

        var vm = GetViewModel(result);
        Assert.False(vm.ResultsLoaded);
    }

    [Fact]
    public async Task Index_NotSubmitted_DoesNotCallRidershipEndpoints()
    {
        var (controller, handler) = Build();

        await controller.Index(null, null, null, null, null, null, submitted: false);

        Assert.False(handler.WasCalled("ridership"));
    }

    [Fact]
    public async Task Index_Submitted_SetsResultsLoaded()
    {
        var (controller, _) = Build(SetupAllRidership);

        var result = await controller.Index(null, null, null, null, null, null, submitted: true);

        Assert.True(GetViewModel(result).ResultsLoaded);
    }

    [Fact]
    public async Task Index_Submitted_PopulatesAllRidershipSections()
    {
        var (controller, _) = Build(SetupAllRidership);

        var result = await controller.Index(null, null, null, null, null, null, submitted: true);

        var vm = GetViewModel(result);
        Assert.NotEmpty(vm.ByMonth);
        Assert.NotEmpty(vm.ByDayOfWeek);
        Assert.NotEmpty(vm.ByHour);
        Assert.NotEmpty(vm.ByStation);
        Assert.NotEmpty(vm.ByBike);
        Assert.NotEmpty(vm.ByBikeType);
        Assert.NotEmpty(vm.TopRoutes);
    }

    [Fact]
    public async Task Index_Submitted_ByMonthDataMatchesApiResponse()
    {
        var (controller, _) = Build(SetupAllRidership);

        var result = await controller.Index(null, null, null, null, null, null, submitted: true);

        var vm = GetViewModel(result);
        Assert.Single(vm.ByMonth);
        Assert.Equal(6,   vm.ByMonth[0].Month);
        Assert.Equal(500, vm.ByMonth[0].TripCount);
    }

    [Fact]
    public async Task Index_Submitted_WithYearFilter_PassesYearInQueryString()
    {
        var (controller, handler) = Build(SetupAllRidership);

        await controller.Index(null, null, year: 2023, null, null, null, submitted: true);

        Assert.True(handler.Requests.Any(r => r.Query.Contains("year=2023")));
    }

    [Fact]
    public async Task Index_Submitted_WithStationFilter_PassesStationInQueryString()
    {
        var (controller, handler) = Build(SetupAllRidership);

        await controller.Index(stationId: 5, null, null, null, null, null, submitted: true);

        Assert.True(handler.Requests.Any(r => r.Query.Contains("stationId=5")));
    }

    [Fact]
    public async Task Index_Submitted_PreservesAllFilterValuesInViewModel()
    {
        var (controller, _) = Build(SetupAllRidership);

        var result = await controller.Index(
            stationId: 3, month: 6, year: 2023, hour: 8, dayOfWeek: 1, bikeTypeId: 2,
            submitted: true);

        var vm = GetViewModel(result);
        Assert.Equal(3,    vm.StationId);
        Assert.Equal(6,    vm.Month);
        Assert.Equal(2023, vm.Year);
        Assert.Equal(8,    vm.Hour);
        Assert.Equal(1,    vm.DayOfWeek);
        Assert.Equal(2,    vm.BikeTypeId);
    }

    [Fact]
    public async Task Index_StationsApiDown_ReturnsEmptyStationList()
    {
        var handler = new FakeHttpMessageHandler(); // no routes — all 404
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("IndegoBikeAPI")).Returns(client);
        var controller = new HomeController(NullLogger<HomeController>.Instance, factory.Object);

        var result = await controller.Index(null, null, null, null, null, null, submitted: false);

        Assert.Empty(GetViewModel(result).Stations);
    }

    [Fact]
    public async Task Index_Submitted_RidershipApiDown_ReturnsEmptyResultsGracefully()
    {
        var (controller, _) = Build(); // stations only — ridership returns 404

        var result = await controller.Index(null, null, null, null, null, null, submitted: true);

        var vm = GetViewModel(result);
        Assert.True(vm.ResultsLoaded);
        Assert.Empty(vm.ByMonth);
        Assert.Empty(vm.ByDayOfWeek);
        Assert.Empty(vm.ByHour);
        Assert.Empty(vm.ByStation);
        Assert.Empty(vm.ByBike);
        Assert.Empty(vm.ByBikeType);
        Assert.Empty(vm.TopRoutes);
    }

    [Fact]
    public async Task Index_InvalidModelState_ReturnsBadRequest()
    {
        var (controller, _) = Build();
        controller.ModelState.AddModelError("stationId", "Invalid value");

        var result = await controller.Index(null, null, null, null, null, null, submitted: false);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Index_Submitted_FetchThrows_ReturnsGracefullyWithEmptyLists()
    {
        var handler = new ThrowingHttpMessageHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://api.test/") };
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient("IndegoBikeAPI")).Returns(client);
        var controller = new HomeController(NullLogger<HomeController>.Instance, factory.Object);

        var result = await controller.Index(null, null, null, null, null, null, submitted: true);

        var vm = GetViewModel(result);
        Assert.Empty(vm.Stations);
        Assert.True(vm.ResultsLoaded);
        Assert.Empty(vm.ByMonth);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var (controller, _) = Build();

        var result = controller.Privacy();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Error_WhenNoActivity_UsesHttpContextTraceIdentifier()
    {
        var (controller, _) = Build();
        Activity.Current = null;
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        var result = controller.Error();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ErrorViewModel>(view.Model);
        Assert.Equal(httpContext.TraceIdentifier, model.RequestId);
    }

    [Fact]
    public void Error_WhenActivityIsActive_UsesActivityId()
    {
        var (controller, _) = Build();
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        var activity = new Activity("TestOperation").Start();
        try
        {
            var result = controller.Error();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(view.Model);
            Assert.Equal(activity.Id, model.RequestId);
        }
        finally
        {
            activity.Stop();
        }
    }
}
