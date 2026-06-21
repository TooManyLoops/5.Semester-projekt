using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Endpoints;
using Timegrip.Gateway.Api.Aggregation.Services;
using Timegrip.Gateway.Api.Observability;
using Timegrip.Gateway.Api.Proxy;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();

// CorrelationContext is scoped per incoming request and reused by downstream HTTP handlers.
builder.Services.AddScoped<CorrelationContext>();
builder.Services.AddTransient<DownstreamLoggingHandler>();

// Typed clients are the gateway's controlled access points to internal services.
// Resilience and downstream logging are applied here so aggregation code stays focused on composition.
builder.Services
    .AddHttpClient<EmployeesApiClient>(client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient<ShiftsApiClient>(client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("SHIFTS_API_URL", "http://localhost:5002"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient<ImportApiClient>(client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("IMPORT_API_URL", "http://localhost:5003"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

// Named clients are used by proxy routes to preserve existing CRUD endpoints through the gateway.
builder.Services
    .AddHttpClient("EmployeesProxy", client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("EMPLOYEES_API_URL", "http://localhost:5001"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

builder.Services
    .AddHttpClient("ShiftsProxy", client =>
    {
        client.BaseAddress = new Uri(GetServiceBaseUrl("SHIFTS_API_URL", "http://localhost:5002"));
    })
    .AddHttpMessageHandler<DownstreamLoggingHandler>()
    .AddStandardResilienceHandler();

// Aggregation services compose frontend-specific read models from downstream service data.
builder.Services.AddScoped<EmployeeAggregationService>();
builder.Services.AddScoped<ShiftAggregationService>();
builder.Services.AddScoped<ShiftImportService>();
builder.Services.AddScoped<RoleCatalogService>();
builder.Services.AddScoped<ProxyService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins(GetFrontendOrigins())
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    );
});

var app = builder.Build();

// Every request gets a correlation id before it reaches endpoints or downstream clients.
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/health",
    () => Results.Ok(new { Status = "Healthy", Service = "Aggregated API Gateway" })
);

app.MapAggregateEndpoints();
app.MapProxyEndpoints();

app.Run();

static string GetServiceBaseUrl(string environmentVariableName, string fallback)
{
    // Docker compose supplies service URLs; local development falls back to localhost ports.
    return Environment.GetEnvironmentVariable(environmentVariableName)?.TrimEnd('/') ?? fallback;
}

static string[] GetFrontendOrigins()
{
    return (Environment.GetEnvironmentVariable("FRONTEND_ORIGIN")
            ?? "http://localhost:62892;http://127.0.0.1:62892")
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}

public partial class Program;
