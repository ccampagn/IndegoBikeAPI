namespace IndegoBikeAPI.Models;

public class TripFilterParams
{
    public int? StationId { get; set; }
    public int? BikeId { get; set; }
    public int? BikeTypeId { get; set; }
    public int? PassTypeId { get; set; }
    public int? Month { get; set; }
    public int? Year { get; set; }
    public int? DayOfWeek { get; set; }
    public int? Hour { get; set; }
}
