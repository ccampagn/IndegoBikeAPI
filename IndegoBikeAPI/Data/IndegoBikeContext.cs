using IndegoBikeAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace IndegoBikeAPI.Data;

public class IndegoBikeContext(DbContextOptions<IndegoBikeContext> options) : DbContext(options)
{
    public DbSet<Station> Stations { get; set; }
}
