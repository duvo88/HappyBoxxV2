using HappyBoxx.ServiceDefaults.Security;

var builder = WebApplication.CreateBuilder(args);

var appServicePort = Environment.GetEnvironmentVariable("PORT")
    ?? Environment.GetEnvironmentVariable("WEBSITES_PORT");
if (!string.IsNullOrWhiteSpace(appServicePort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{appServicePort}");
}

builder.AddServiceDefaults();
builder.Services.AddHappyBoxxAuthentication(builder.Configuration, builder.Environment);

var allowedCorsOrigins = (builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
    .Select(origin => origin.Trim().TrimEnd('/'))
    .Where(origin => !string.IsNullOrWhiteSpace(origin))
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();
var hasCorsOrigins = allowedCorsOrigins.Any();
if (hasCorsOrigins)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("FrontendCors", policy =>
        {
            policy.WithOrigins(allowedCorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (hasCorsOrigins)
{
    app.UseCors("FrontendCors");
}

app.UseAuthentication();
app.UseAuthorization();
app.MapDefaultEndpoints();
app.MapReverseProxy();

await app.RunAsync();
