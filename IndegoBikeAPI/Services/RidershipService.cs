using IndegoBikeAPI.Data;
using IndegoBikeAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Services;

public class RidershipService(IndegoBikeContext db) : IRidershipService
{
    public async Task<IEnumerable<RidershipByMonthDto>> GetRidershipByMonthAsync(TripFilterParams filters)
    {
        var (join, where, p) = BuildClauses(filters);
#pragma warning disable EF1002
        return await db.Database.SqlQueryRaw<RidershipByMonthDto>($"""
            SELECT MONTH(t.StartDate) AS Month, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY MONTH(t.StartDate)
            ORDER BY Month
            """, p).ToListAsync();
    }

    public async Task<IEnumerable<RidershipByDayOfWeekDto>> GetRidershipByDayOfWeekAsync(TripFilterParams filters)
    {
        var (join, where, p) = BuildClauses(filters);
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
        var (join, where, p) = BuildClauses(filters);
        return await db.Database.SqlQueryRaw<RidershipByHourDto>($"""
            SELECT DATEPART(HOUR, t.StartDate) AS Hour, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY DATEPART(HOUR, t.StartDate)
            ORDER BY Hour
            """, p).ToListAsync();
    }

    public async Task<IEnumerable<RidershipByStationDto>> GetRidershipByStationAsync(TripFilterParams filters)
    {
        var (join, where, p) = BuildClauses(filters);
        return await db.Database.SqlQueryRaw<RidershipByStationDto>($"""
            SELECT t.StartStationID AS StationID, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY t.StartStationID
            ORDER BY TripCount DESC
            """, p).ToListAsync();
    }

    public async Task<IEnumerable<RidershipByBikeDto>> GetRidershipByBikeAsync(TripFilterParams filters)
    {
        var (join, where, p) = BuildClauses(filters);
        return await db.Database.SqlQueryRaw<RidershipByBikeDto>($"""
            SELECT t.BikeID, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY t.BikeID
            ORDER BY TripCount DESC
            """, p).ToListAsync();
    }

    public async Task<IEnumerable<RidershipByBikeTypeDto>> GetRidershipByBikeTypeAsync(TripFilterParams filters)
    {
        var (join, where, p) = BuildClauses(filters, joinBike: true);
        return await db.Database.SqlQueryRaw<RidershipByBikeTypeDto>($"""
            SELECT b.BikeTypeID, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY b.BikeTypeID
            ORDER BY b.BikeTypeID
            """, p).ToListAsync();
    }

    public async Task<IEnumerable<TopRoutePairingDto>> GetTopRoutePairingsAsync(TripFilterParams filters)
    {
        var (join, where, p) = BuildClauses(filters);
        return await db.Database.SqlQueryRaw<TopRoutePairingDto>($"""
            SELECT TOP 50 t.StartStationID, t.EndStationID, COUNT(*) AS TripCount
            FROM Trip t {join}
            {where}
            GROUP BY t.StartStationID, t.EndStationID
            ORDER BY TripCount DESC
            """, p).ToListAsync();
    }

    private static (string join, string where, SqlParameter[] parameters) BuildClauses(
        TripFilterParams filters, bool joinBike = false)
    {
        var conditions = new List<string>();
        var parameters = new List<SqlParameter>();
        var needsBikeJoin = joinBike;

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

        var join  = needsBikeJoin    ? "JOIN Bike b ON t.BikeID = b.BikeID" : "";
        var where = conditions.Count > 0 ? "WHERE " + string.Join(" AND ", conditions) : "";
        return (join, where, parameters.ToArray());
    }
}
