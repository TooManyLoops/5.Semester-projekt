using Microsoft.AspNetCore.Http.HttpResults;
using Vagtplanlægnings_modul.Requests;
using Vagtplanlægnings_modul.Services;

namespace Vagtplanlægnings_modul.Endpoints
{
    public static class ShiftEndpoints
    {
        public static WebApplication MapShiftEndpoints(this WebApplication app)
        {
            var ShiftEndpoint = app.MapGroup("/Shift");
            ShiftEndpoint.MapPost("/", CreateShift).WithName("CreateShift");

            return app;
        }

        public static async Task<IResult> CreateShift(
            ShiftService service,
            ShiftRequest request
        )
        {
            var result = await service.CreateShift(request);
            return TypedResults.Ok(result);
        }

    }
}
