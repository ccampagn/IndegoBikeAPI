using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Services;

public class RidershipService(IndegoBikeContext db) : IRidershipService
{
    // 2000-01-02 is a Sunday; DATEDIFF(day, ref, date) % 7 → 0=Sun…6=Sat (matches DayOfWeek enum)
    private static readonly DateTime SundayRef = new(2000, 1, 2);

    public async Task<IEnumerable<RidershipByMonthDto>> GetRidershipByMonthAsync(TripFilterParams f) =>
        
        await ApplyFilters(f)
            .GroupBy(t => t.StartDate.Month)
            .Select(g => new RidershipByMonthDto { Month = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.Month)
            .ToListAsync();

    public async Task<IEnumerable<RidershipByDayOfWeekDto>> GetRidershipByDayOfWeekAsync(TripFilterParams f) =>
        await ApplyFilters(f)
            .GroupBy(t => EF.Functions.DateDiffDay(SundayRef, t.StartDate) % 7)
            .Select(g => new RidershipByDayOfWeekDto { DayOfWeek = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.DayOfWeek)
            .ToListAsync();

    public async Task<IEnumerable<RidershipByHourDto>> GetRidershipByHourAsync(TripFilterParams f) =>
        await ApplyFilters(f)
            .GroupBy(t => t.StartDate.Hour)
            .Select(g => new RidershipByHourDto { Hour = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.Hour)
            .ToListAsync();

    public async Task<IEnumerable<RidershipByStationDto>> GetRidershipByStationAsync(TripFilterParams f) =>
        await ApplyFilters(f)
            .GroupBy(t => t.StartStationID)
            .Select(g => new RidershipByStationDto { StationID = g.Key, TripCount = g.Count() })
            .OrderByDescending(r => r.TripCount)
            .ToListAsync();

    public async Task<IEnumerable<RidershipByBikeDto>> GetRidershipByBikeAsync(TripFilterParams f) =>
        await ApplyFilters(f)
            .GroupBy(t => t.BikeID)
            .Select(g => new RidershipByBikeDto { BikeID = g.Key, TripCount = g.Count() })
            .OrderByDescending(r => r.TripCount)
            .ToListAsync();

    public async Task<IEnumerable<RidershipByBikeTypeDto>> GetRidershipByBikeTypeAsync(TripFilterParams f) =>
        await ApplyFilters(f)
            .Join(db.Bikes, t => t.BikeID, b => b.BikeID, (t, b) => b.BikeTypeID)
            .GroupBy(bikeTypeId => bikeTypeId)
            .Select(g => new RidershipByBikeTypeDto { BikeTypeID = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.BikeTypeID)
            .ToListAsync();

    public async Task<IEnumerable<TopRoutePairingDto>> GetTopRoutePairingsAsync(TripFilterParams f) =>
        await ApplyFilters(f)
            .GroupBy(t => new { t.StartStationID, t.EndStationID })
            .Select(g => new TopRoutePairingDto
            {
                StartStationID = g.Key.StartStationID,
                EndStationID = g.Key.EndStationID,
                TripCount = g.Count()
            })
            .OrderByDescending(r => r.TripCount)
            .Take(50)
            .ToListAsync();

    private IQueryable<Trip> ApplyFilters(TripFilterParams f)
    {
        var query = db.Trips.AsQueryable();

        if (f.StationId.HasValue)
            query = query.Where(t => t.StartStationID == f.StationId || t.EndStationID == f.StationId);
        if (f.BikeId.HasValue)
            query = query.Where(t => t.BikeID == f.BikeId);
        if (f.BikeTypeId.HasValue)
            query = query.Where(t => db.Bikes.Any(b => b.BikeID == t.BikeID && b.BikeTypeID == f.BikeTypeId));
        if (f.PassTypeId.HasValue)
            query = query.Where(t => t.PassPlanID == f.PassTypeId);
        if (f.Month.HasValue)
            query = query.Where(t => t.StartDate.Month == f.Month);
        if (f.Year.HasValue)
            query = query.Where(t => t.StartDate.Year == f.Year);
        if (f.DayOfWeek.HasValue)
            query = query.Where(t => EF.Functions.DateDiffDay(SundayRef, t.StartDate) % 7 == f.DayOfWeek.Value);
        if (f.Hour.HasValue)
            query = query.Where(t => t.StartDate.Hour == f.Hour);

        return query;
    }
}
