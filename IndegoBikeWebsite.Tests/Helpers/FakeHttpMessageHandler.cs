using System.Net;
using System.Text;
using System.Text.Json;

namespace IndegoBikeWebsite.Tests.Helpers;

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly List<(string PathEnding, HttpStatusCode Status, string? Json)> _routes = [];
    private readonly List<Uri> _requests = [];
    private readonly object _lock = new();

    // Register more-specific patterns before less-specific ones (e.g. "by-bike-type" before "by-bike").
    public void Setup(string pathEnding, HttpStatusCode status, object? body = null)
    {
        var json = body is null ? null : JsonSerializer.Serialize(body);
        _routes.Add((pathEnding, status, json));
    }

    public IReadOnlyList<Uri> Requests
    {
        get { lock (_lock) return [.. _requests]; }
    }

    public bool WasCalled(string pathContains) =>
        Requests.Any(r => r.AbsolutePath.Contains(pathContains));

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var uri = request.RequestUri!;
        lock (_lock) _requests.Add(uri);

        var path = uri.AbsolutePath;
        var match = _routes.FirstOrDefault(r => path.EndsWith(r.PathEnding));

        var response = new HttpResponseMessage(
            match.PathEnding is not null ? match.Status : HttpStatusCode.NotFound);

        if (match.Json is not null)
            response.Content = new StringContent(match.Json, Encoding.UTF8, "application/json");

        return Task.FromResult(response);
    }
}
