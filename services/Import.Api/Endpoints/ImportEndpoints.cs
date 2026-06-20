using System.Text.Json;
using Timegrip.Import.Api.Requests;
using Timegrip.Import.Api.Services;

namespace Timegrip.Import.Api.Endpoints;

public static class ImportEndpoints
{
    public static WebApplication MapImportEndpoints(this WebApplication app)
    {
        var imports = app.MapGroup("/imports");

        imports.MapPost(
                "/shifts/parse",
                async (
                    ShiftImportService service,
                    IFormFile file,
                    string roles,
                    CancellationToken cancellationToken
                ) =>
                {
                    try
                    {
                        var roleCatalog = JsonSerializer.Deserialize<List<RoleReferenceRequest>>(
                            roles,
                            new JsonSerializerOptions(JsonSerializerDefaults.Web)
                        ) ?? [];

                        var result = await service.ParseShifts(
                            file,
                            roleCatalog,
                            cancellationToken
                        );

                        return result.IsValid ? Results.Ok(result) : Results.BadRequest(result);
                    }
                    catch (InvalidDataException exception)
                    {
                        return Results.BadRequest(new { Message = exception.Message });
                    }
                    catch (InvalidOperationException exception)
                    {
                        return Results.BadRequest(new { Message = exception.Message });
                    }
                    catch (JsonException exception)
                    {
                        return Results.BadRequest(new { Message = exception.Message });
                    }
                }
            )
            .DisableAntiforgery();

        return app;
    }
}
