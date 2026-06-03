using IndegoBikeAPI.Models;
using IndegoBikeAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IndegoBikeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RidershipController(IRidershipService ridership) : ControllerBase
{
    [HttpGet("by-month")]
    public async Task<ActionResult<IEnumerable<RidershipByMonthDto>>> GetByMonth([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetRidershipByMonthAsync(filters));

    [HttpGet("by-day-of-week")]
    public async Task<ActionResult<IEnumerable<RidershipByDayOfWeekDto>>> GetByDayOfWeek([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetRidershipByDayOfWeekAsync(filters));

    [HttpGet("by-hour")]
    public async Task<ActionResult<IEnumerable<RidershipByHourDto>>> GetByHour([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetRidershipByHourAsync(filters));

    [HttpGet("by-station")]
    public async Task<ActionResult<IEnumerable<RidershipByStationDto>>> GetByStation([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetRidershipByStationAsync(filters));

    [HttpGet("by-bike")]
    public async Task<ActionResult<IEnumerable<RidershipByBikeDto>>> GetByBike([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetRidershipByBikeAsync(filters));

    [HttpGet("by-bike-type")]
    public async Task<ActionResult<IEnumerable<RidershipByBikeTypeDto>>> GetByBikeType([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetRidershipByBikeTypeAsync(filters));

    [HttpGet("top-routes")]
    public async Task<ActionResult<IEnumerable<TopRoutePairingDto>>> GetTopRoutes([FromQuery] TripFilterParams filters) =>
        Ok(await ridership.GetTopRoutePairingsAsync(filters));
}
