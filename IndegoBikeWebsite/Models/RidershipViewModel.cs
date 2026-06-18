namespace IndegoBikeWebsite.Models;

public record StationDto(int StationID, string? StationName);
public record ByMonthRow(int Month, int TripCount);
public record ByDayRow(int DayOfWeek, int TripCount);
public record ByHourRow(int Hour, int TripCount);
public record ByStationRow(int? StationID, int TripCount);
public record ByBikeRow(int? BikeID, int TripCount);
public record ByBikeTypeRow(int? BikeTypeID, int TripCount);
public record TopRouteRow(int? StartStationID, int? EndStationID, int TripCount);

public class RidershipViewModel
{
    public List<StationDto> Stations { get; set; } = [];
    public int? StationId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? Hour { get; set; }
    public int? DayOfWeek { get; set; }
    public int? BikeTypeId { get; set; }
    public bool ResultsLoaded { get; set; }
    public List<ByMonthRow> ByMonth { get; set; } = [];
    public List<ByDayRow> ByDayOfWeek { get; set; } = [];
    public List<ByHourRow> ByHour { get; set; } = [];
    public List<ByStationRow> ByStation { get; set; } = [];
    public List<ByBikeRow> ByBike { get; set; } = [];
    public List<ByBikeTypeRow> ByBikeType { get; set; } = [];
    public List<TopRouteRow> TopRoutes { get; set; } = [];
}
