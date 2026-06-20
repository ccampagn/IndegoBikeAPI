using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Services;

public class RidershipService(IndegoBikeContext db) : IRidershipService
{
    public async Task<IEnumerable<RidershipByMonthDto>> GetRidershipByMonthAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
            .GroupBy(t => t.StartDate.Month)
            .Select(g => new RidershipByMonthDto { Month = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.Month)
            .ToListAsync(); 
    }

    public async Task<IEnumerable<RidershipByDayOfWeekDto>> GetRidershipByDayOfWeekAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
            .GroupBy(t => (int)t.StartDate.DayOfWeek)
            .Select(g => new RidershipByDayOfWeekDto { DayOfWeek = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.DayOfWeek)
            .ToListAsync();
    }

    public async Task<IEnumerable<RidershipByHourDto>> GetRidershipByHourAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
            .GroupBy(t => t.StartDate.Hour)
            .Select(g => new RidershipByHourDto { Hour = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.Hour)
            .ToListAsync();
    }

    public async Task<IEnumerable<RidershipByStationDto>> GetRidershipByStationAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
            .GroupBy(t => t.StartStationID)
            .Select(g => new RidershipByStationDto { StationID = g.Key, TripCount = g.Count() })
            .OrderByDescending(r => r.TripCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<RidershipByBikeDto>> GetRidershipByBikeAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
            .GroupBy(t => t.BikeID)
            .Select(g => new RidershipByBikeDto { BikeID = g.Key, TripCount = g.Count() })
            .OrderByDescending(r => r.TripCount)
            .ToListAsync();
    }

    public async Task<IEnumerable<RidershipByBikeTypeDto>> GetRidershipByBikeTypeAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
            .Join(db.Bikes, t => t.BikeID, b => b.BikeID, (t, b) => b.BikeTypeID)
            .GroupBy(typeId => typeId)
            .Select(g => new RidershipByBikeTypeDto { BikeTypeID = g.Key, TripCount = g.Count() })
            .OrderBy(r => r.BikeTypeID)
            .ToListAsync();
    }

    public async Task<IEnumerable<TopRoutePairingDto>> GetTopRoutePairingsAsync(TripFilterParams filters)
    {
        var query = ApplyFilters(db.Trips.AsQueryable(), filters);
        return await query
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
    }

    private IQueryable<Trip> ApplyFilters(IQueryable<Trip> query, TripFilterParams filters)
    {
        if (filters.StationId.HasValue)
            query = query.Where(t => t.StartStationID == filters.StationId || t.EndStationID == filters.StationId);
        if (filters.BikeId.HasValue)
            query = query.Where(t => t.BikeID == filters.BikeId);
        if (filters.BikeTypeId.HasValue)
            query = query.Where(t => db.Bikes.Any(b => b.BikeID == t.BikeID && b.BikeTypeID == filters.BikeTypeId));
        if (filters.PassTypeId.HasValue)
            query = query.Where(t => t.PassPlanID == filters.PassTypeId);
        if (filters.Month.HasValue)
            query = query.Where(t => t.StartDate.Month == filters.Month);
        if (filters.Year.HasValue)
            query = query.Where(t => t.StartDate.Year == filters.Year);
        if (filters.DayOfWeek.HasValue)
            query = query.Where(t => (int)t.StartDate.DayOfWeek == filters.DayOfWeek);
        if (filters.Hour.HasValue)
            query = query.Where(t => t.StartDate.Hour == filters.Hour);
        return query;
    }
}
