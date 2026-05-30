using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Services;

public class StationService(IndegoBikeContext db) : IStationService
{
    public async Task<IEnumerable<Station>> GetAllAsync() =>
        await db.Stations.ToListAsync();

    public async Task<Station?> GetByIdAsync(int id) =>
        await db.Stations.FindAsync(id);
}
