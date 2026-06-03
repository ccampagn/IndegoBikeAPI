using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndegoBikeAPI.Models;

[Table("Trip")]
public class Trip
{
    [Key]
    public long TripID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? StartStationID { get; set; }
    public int? EndStationID { get; set; }
    public int? BikeID { get; set; }
    public int? PassPlanID { get; set; }
}
