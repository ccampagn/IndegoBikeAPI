using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndegoBikeAPI.Models;

[Table("Station")]
public class Station
{
    [Key]
    public int StationID { get; set; }
    public string? StationName { get; set; }
    public DateOnly? GoLiveDate { get; set; }
    public decimal? Latitude { get; set; }
    public decimal? Longitude { get; set; }
    public bool? IsActive { get; set; }
}
