using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.Data.SqlClient;
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
        var (join, where, p) = BuildRawClauses(filters);
#pragma warning disable EF1002
        return await db.Database.SqlQueryRaw<RidershipByDayOfWeekDto>($"""
            SELECT DATEDIFF(day, '20000102', t.StartDate) % 7 AS DayOfWeek, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY DATEDIFF(day, '20000102', t.StartDate) % 7
            ORDER BY DayOfWeek
            """, p).ToListAsync();
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
        if (filters.Hour.HasValue)
            query = query.Where(t => t.StartDate.Hour == filters.Hour);
        return query;
    }

    private static (string join, string where, SqlParameter[] parameters) BuildRawClauses(TripFilterParams filters)
    {
        var conditions = new List<string>();
        var parameters = new List<SqlParameter>();
        var needsBikeJoin = false;

        if (filters.StationId.HasValue)
        {
            conditions.Add("(t.StartStationID = @StationId OR t.EndStationID = @StationId)");
            parameters.Add(new SqlParameter("@StationId", filters.StationId.Value));
        }
        if (filters.BikeId.HasValue)
        {
            conditions.Add("t.BikeID = @BikeId");
            parameters.Add(new SqlParameter("@BikeId", filters.BikeId.Value));
        }
        if (filters.BikeTypeId.HasValue)
        {
            needsBikeJoin = true;
            conditions.Add("b.BikeTypeID = @BikeTypeId");
            parameters.Add(new SqlParameter("@BikeTypeId", filters.BikeTypeId.Value));
        }
        if (filters.PassTypeId.HasValue)
        {
            conditions.Add("t.PassPlanID = @PassTypeId");
            parameters.Add(new SqlParameter("@PassTypeId", filters.PassTypeId.Value));
        }
        if (filters.Month.HasValue)
        {
            conditions.Add("MONTH(t.StartDate) = @Month");
            parameters.Add(new SqlParameter("@Month", filters.Month.Value));
        }
        if (filters.Year.HasValue)
        {
            conditions.Add("YEAR(t.StartDate) = @Year");
            parameters.Add(new SqlParameter("@Year", filters.Year.Value));
        }
        if (filters.DayOfWeek.HasValue)
        {
            conditions.Add("DATEDIFF(day, '20000102', t.StartDate) % 7 = @DayOfWeek");
            parameters.Add(new SqlParameter("@DayOfWeek", filters.DayOfWeek.Value));
        }
        if (filters.Hour.HasValue)
        {
            conditions.Add("DATEPART(HOUR, t.StartDate) = @Hour");
            parameters.Add(new SqlParameter("@Hour", filters.Hour.Value));
        }

        var join  = needsBikeJoin ? "JOIN Bike b ON t.BikeID = b.BikeID" : "";
        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        return (join, where, parameters.ToArray());
    }
}
