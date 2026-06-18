using IndegoBikeAPI.Models;
using IndegoBikeAPI.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace IndegoBikeAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableCors("IndegoBikeWebsite")]
public class StationsController(IStationService stations) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Station>>> GetAll() =>
        Ok(await stations.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Station>> GetById(int id)
    {
        var station = await stations.GetByIdAsync(id);
        return station is null ? NotFound() : Ok(station);
    }
}
