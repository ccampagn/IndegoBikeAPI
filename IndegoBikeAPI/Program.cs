using IndegoBikeAPI.Data;
using IndegoBikeAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IndegoBikeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IndegoBike")));

builder.Services.AddScoped<IStationService, StationService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
