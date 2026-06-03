using IndegoBikeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Data;

public class IndegoBikeContext(DbContextOptions<IndegoBikeContext> options) : DbContext(options)
{
    public DbSet<Station> Stations { get; set; }
    public DbSet<Trip> Trips { get; set; }
    public DbSet<Bike> Bikes { get; set; }
}
