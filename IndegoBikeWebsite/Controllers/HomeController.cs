using IndegoBikeWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace IndegoBikeWebsite.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

        public HomeController(ILogger<HomeController> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(
            int? stationId, int? month, int? year, int? hour, int? dayOfWeek, int? bikeTypeId,
            bool submitted = false)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var client = _httpClientFactory.CreateClient("IndegoBikeAPI");

            var vm = new RidershipViewModel
            {
                Stations = await Fetch<List<StationDto>>(client, "api/stations") ?? [],
                StationId = stationId,
                Month = month,
                Year = year,
                Hour = hour,
                DayOfWeek = dayOfWeek,
                BikeTypeId = bikeTypeId
            };

            if (submitted)
            {
                var qs = BuildQs(stationId, month, year, hour, dayOfWeek, bikeTypeId);

                var byMonthTask      = Fetch<List<ByMonthRow>>(client,    $"api/ridership/by-month{qs}");
                var byDayTask        = Fetch<List<ByDayRow>>(client,      $"api/ridership/by-day-of-week{qs}");
                var byHourTask       = Fetch<List<ByHourRow>>(client,     $"api/ridership/by-hour{qs}");
                var byStationTask    = Fetch<List<ByStationRow>>(client,  $"api/ridership/by-station{qs}");
                var byBikeTask       = Fetch<List<ByBikeRow>>(client,     $"api/ridership/by-bike{qs}");
                var byBikeTypeTask   = Fetch<List<ByBikeTypeRow>>(client, $"api/ridership/by-bike-type{qs}");
                var topRoutesTask    = Fetch<List<TopRouteRow>>(client,   $"api/ridership/top-routes{qs}");

                await Task.WhenAll(byMonthTask, byDayTask, byHourTask, byStationTask, byBikeTask, byBikeTypeTask, topRoutesTask);

                vm.ByMonth      = await byMonthTask    ?? [];
                vm.ByDayOfWeek  = await byDayTask      ?? [];
                vm.ByHour       = await byHourTask     ?? [];
                vm.ByStation    = await byStationTask  ?? [];
                vm.ByBike       = await byBikeTask     ?? [];
                vm.ByBikeType   = await byBikeTypeTask ?? [];
                vm.TopRoutes    = await topRoutesTask  ?? [];
                vm.ResultsLoaded = true;
            }

            return View(vm);
        }

        private async Task<T?> Fetch<T>(HttpClient client, string url)
        {
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode) return default;
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(json, JsonOpts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch {Url}", url);
                return default;
            }
        }

        private static string BuildQs(int? stationId, int? month, int? year, int? hour, int? dayOfWeek, int? bikeTypeId)
        {
            var parts = new List<string>();
            if (stationId.HasValue)  parts.Add($"stationId={stationId}");
            if (month.HasValue)      parts.Add($"month={month}");
            if (year.HasValue)       parts.Add($"year={year}");
            if (hour.HasValue)       parts.Add($"hour={hour}");
            if (dayOfWeek.HasValue)  parts.Add($"dayOfWeek={dayOfWeek}");
            if (bikeTypeId.HasValue) parts.Add($"bikeTypeId={bikeTypeId}");
            return parts.Count > 0 ? "?" + string.Join("&", parts) : "";
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
