using Timegrip.Import.Api.Endpoints;
using Timegrip.Import.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddScoped<ShiftImportService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet(
    "/health",
    () => Results.Ok(new { Status = "Healthy", Service = "Import API" })
);

app.MapImportEndpoints();

app.Run();

public partial class Program;
