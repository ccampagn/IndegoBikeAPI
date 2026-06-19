using IndegoBikeAPI.Data;
using IndegoBikeAPI.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<IndegoBikeContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("IndegoBike")));

builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IRidershipService, RidershipService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("IndegoBikeWebsite", policy =>
        policy.WithOrigins(
            "https://indegobike-b4bdgcayh9heg7as.centralus-01.azurewebsites.net")
              .AllowAnyMethod()
              .AllowAnyHeader());
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseCors("IndegoBikeWebsite");

app.UseAuthorization();

app.MapControllers().RequireCors("IndegoBikeWebsite");

await app.RunAsync();

public static partial class Program { }