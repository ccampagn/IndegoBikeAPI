using IndegoBikeAPI.Models;

namespace IndegoBikeAPI.Services;

public interface IRidershipService
{
    Task<IEnumerable<RidershipByMonthDto>> GetRidershipByMonthAsync(TripFilterParams filters);
    Task<IEnumerable<RidershipByDayOfWeekDto>> GetRidershipByDayOfWeekAsync(TripFilterParams filters);
    Task<IEnumerable<RidershipByHourDto>> GetRidershipByHourAsync(TripFilterParams filters);
    Task<IEnumerable<RidershipByStationDto>> GetRidershipByStationAsync(TripFilterParams filters);
    Task<IEnumerable<RidershipByBikeDto>> GetRidershipByBikeAsync(TripFilterParams filters);
    Task<IEnumerable<RidershipByBikeTypeDto>> GetRidershipByBikeTypeAsync(TripFilterParams filters);
    Task<IEnumerable<TopRoutePairingDto>> GetTopRoutePairingsAsync(TripFilterParams filters);
}
