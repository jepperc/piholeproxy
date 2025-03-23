using CSharpProxy;

var builder = WebApplication.CreateBuilder(args);
var host = Environment.GetEnvironmentVariable("PIHOLE_HOST") ?? throw new ArgumentNullException("PIHOLE_HOST");
var key = Environment.GetEnvironmentVariable("PIHOLE_API_KEY") ?? throw new ArgumentNullException("PIHOLE_API_KEY");
var app = builder.Build();

// Hjælpefunktion der sender et POST-kald til en given URL med et JSON-body
async Task<IResult> ProxyRequest(string targetUrl, object body, string apiKey)
{
    var handler = new HttpClientHandler() 
    { 
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
    using var client = new HttpClient(handler);
    var auth = await client.PostAsJsonAsync($"{host}/api/auth", new { password = apiKey});
    auth.EnsureSuccessStatusCode();
    var help = await auth.Content.ReadAsStringAsync();
    Console.WriteLine(help);
    var authResp = await auth.Content.ReadFromJsonAsync<AuthResponse>();
    // Hvis apiKey er sat, tilføjes den til Authorization header
    if (!string.IsNullOrWhiteSpace(authResp?.Session.Sid))
    {
        client.DefaultRequestHeaders.Add("sid", $"{authResp?.Session.Sid}");
    }
    var response = await client.PostAsJsonAsync(targetUrl, body);
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
    "/disable300",
    $"{host}/api/dns/blocking",
    new { blocking = false, timer = 300 }
);

MapProxyEndpoint(
    "/disable30",
    $"{host}/api/dns/blocking",
    new { blocking = false, timer = 30 }
);

MapProxyEndpoint(
    "/disable",
    $"{host}/api/dns/blocking",
    new { blocking = false }
);

MapProxyEndpoint(
    "/enable",
    $"{host}/api/dns/blocking",
    new { blocking = true }
);

app.Run();
