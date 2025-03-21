using System.Net.Http;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var host = Environment.GetEnvironmentVariable("PIHOLE_HOST") ?? throw new ArgumentNullException("PIHOLE_HOST");
var key = Environment.GetEnvironmentVariable("PIHOLE_API_KEY") ?? throw new ArgumentNullException("PIHOLE_API_KEY");
var app = builder.Build();

// Hjælpefunktion der sender et POST-kald til en given URL med et JSON-body
async Task<IResult> ProxyRequest(string targetUrl, string jsonBody, string? apiKey)
{
    using var client = new HttpClient();
    // Sæt Content-Type header
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    // Hvis apiKey er sat, tilføjes den til Authorization header
    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
    }
    var response = await client.PostAsync(targetUrl, content);
    var responseContent = await response.Content.ReadAsStringAsync();
    return Results.Json(new {
        target = targetUrl,
        status = response.StatusCode,
        response = responseContent
    });
}

// Hjælpefunktion til at mappe et endpoint
void MapProxyEndpoint(string route, string targetUrl, string jsonBody)
{
    // Læs API-nøgle og eventuelt Pi-hole host fra miljøvariabler
    var apiKey = Environment.GetEnvironmentVariable("PIHOLE_API_KEY") ?? "";
    app.MapGet(route, () => ProxyRequest(targetUrl, jsonBody, apiKey));
}

// Eksempel: /disable endpoint, der omdanner GET til et POST-kald med et JSON-body
MapProxyEndpoint(
    "/disable",
    $"{Environment.GetEnvironmentVariable("PIHOLE_HOST")}/api.php",
    JsonSerializer.Serialize(new { auth = Environment.GetEnvironmentVariable("PIHOLE_API_KEY"), disable = 300 })
);

// Eksempel på et andet endpoint – tilføj så mange du vil
MapProxyEndpoint(
    "/another",
    "http://example.com/another-api",
    "{\"key\":\"value\"}"
);

app.Run();
