using IndegoBikeAPI.Models;

namespace IndegoBikeAPI.Services;

public interface IStationService
{
    Task<IEnumerable<Station>> GetAllAsync();
    Task<Station?> GetByIdAsync(int id);
}
