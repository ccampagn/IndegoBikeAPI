using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IndegoBikeAPI.Models;

[Table("Bike")]
public class Bike
{
    [Key]
    public int BikeID { get; set; }
    public int? BikeTypeID { get; set; }
}
