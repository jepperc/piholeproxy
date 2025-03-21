using System.Net.Http;
using System.Text;
using System.Text.Json;
using CSharpProxy;

var builder = WebApplication.CreateBuilder(args);
var host = Environment.GetEnvironmentVariable("PIHOLE_HOST") ?? throw new ArgumentNullException("PIHOLE_HOST");
var key = Environment.GetEnvironmentVariable("PIHOLE_API_KEY") ?? throw new ArgumentNullException("PIHOLE_API_KEY");
var app = builder.Build();

// Hjælpefunktion der sender et POST-kald til en given URL med et JSON-body
async Task<IResult> ProxyRequest(string targetUrl, object body, string apiKey)
{
    using var client = new HttpClient();
    var auth = await client.PostAsJsonAsync($"{host}/auth", new { password = apiKey});
    auth.EnsureSuccessStatusCode();
    var authResp = await auth.Content.ReadFromJsonAsync<AuthResponse>();
    // Sæt Content-Type header
    //var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    // Hvis apiKey er sat, tilføjes den til Authorization header
    if (!string.IsNullOrWhiteSpace(authResp?.Session.Sid))
    {
        client.DefaultRequestHeaders.Add("sid", $"{authResp?.Session.Sid}");
    }
    var response = await client.PostAsJsonAsync(targetUrl, body);
    //var response = await client.PostAsync(targetUrl, content);
    var responseContent = await response.Content.ReadAsStringAsync();
    return Results.Json(new {
        target = targetUrl,
        status = response.StatusCode,
        response = responseContent
    });
}

// Hjælpefunktion til at mappe et endpoint
void MapProxyEndpoint(string route, string targetUrl, object body)
{
    // Læs API-nøgle og eventuelt Pi-hole host fra miljøvariabler
    app.MapGet(route, () => ProxyRequest(targetUrl, body, key));
}

// Eksempel: /disable endpoint, der omdanner GET til et POST-kald med et JSON-body
MapProxyEndpoint(
    "/disable",
    $"{host}/api/dns/blocking",
    new { blocking = false, timer = 300 }
);

MapProxyEndpoint(
    "/disable30",
    $"{host}/api/dns/blocking",
    new { blocking = false, timer = 30 }
);

MapProxyEndpoint(
    "/disableperm",
    $"{host}/api/dns/blocking",
    new { blocking = false }
);

MapProxyEndpoint(
    "/enable",
    $"{host}/api/dns/blocking",
    new { blocking = true }
);

app.Run();
