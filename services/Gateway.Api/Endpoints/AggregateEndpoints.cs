using System.Net;
using Timegrip.Gateway.Api.Downstream;
using Timegrip.Gateway.Api.Aggregation.Requests;
using Timegrip.Gateway.Api.Aggregation.Services;

namespace Timegrip.Gateway.Api.Endpoints;

public static class AggregateEndpoints
{
    public static WebApplication MapAggregateEndpoints(this WebApplication app)
    {
        // Aggregate endpoints are frontend-facing read/use-case endpoints.
        // They may call multiple downstream services before returning one response.
        var aggregate = app.MapGroup("/api/aggregate");

        aggregate.MapGet(
            "/employees",
            async (EmployeeAggregationService service, CancellationToken cancellationToken) =>
                await Execute(() => service.GetEmployees(cancellationToken))
        );

        aggregate.MapGet(
            "/employees/{employeeId:guid}",
            async (
                EmployeeAggregationService service,
                Guid employeeId,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    var result = await service.GetEmployeeDetails(employeeId, cancellationToken);
                    return result is null
                        ? Results.NotFound(new { Message = "Employee was not found.", EmployeeId = employeeId })
                        : Results.Ok(result);
                }
                catch (Exception exception) when (TryMapGatewayException(exception, out var mapped))
                {
                    return mapped;
                }
            }
        );

        aggregate.MapGet(
            "/forms/create-shift",
            async (EmployeeAggregationService service, CancellationToken cancellationToken) =>
                await Execute(() => service.GetCreateShiftForm(cancellationToken))
        );

        aggregate.MapGet(
            "/shifts",
            async (ShiftAggregationService service, CancellationToken cancellationToken) =>
                await Execute(() => service.GetShifts(cancellationToken))
        );

        aggregate.MapPost(
            "/shifts/{shiftId:guid}/assign",
            async (
                ShiftAggregationService service,
                Guid shiftId,
                AssignShiftGatewayRequest request,
                CancellationToken cancellationToken
            ) =>
            {
                try
                {
                    // The gateway verifies cross-service references before forwarding the write.
                    var result = await service.AssignShift(shiftId, request, cancellationToken);
                    return result == ShiftAssignmentResult.EmployeeRoleNotFound
                        ? Results.NotFound(
                            new
                            {
                                Message = "Employee role was not found.",
                                request.EmployeeRoleId,
                            }
                        )
                        : Results.Ok();
                }
                catch (Exception exception) when (TryMapGatewayException(exception, out var mapped))
                {
                    return mapped;
                }
            }
        );

        aggregate.MapPost(
                "/imports/shifts",
                async (
                    ShiftImportService service,
                    IFormFile file,
                    bool includePast,
                    bool excludePast,
                    CancellationToken cancellationToken
                ) =>
                {
                    try
                    {
                        var result = await service.ImportShifts(
                            file,
                            includePast,
                            excludePast,
                            cancellationToken
                        );
                        return result.IsValid ? Results.Ok(result) : Results.BadRequest(result);
                    }
                    catch (Exception exception) when (TryMapGatewayException(exception, out var mapped))
                    {
                        return mapped;
                    }
                }
            )
            .DisableAntiforgery();

        return app;
    }

    private static async Task<IResult> Execute<T>(Func<Task<T>> action)
    {
        try
        {
            return Results.Ok(await action());
        }
        catch (Exception exception) when (TryMapGatewayException(exception, out var mapped))
        {
            return mapped;
        }
    }

    private static bool TryMapGatewayException(Exception exception, out IResult result)
    {
        // Keep downstream details behind the gateway while preserving useful HTTP status codes.
        result = exception switch
        {
            DownstreamUnavailableException unavailable => Results.Problem(
                title: $"{unavailable.ServiceName} is unavailable.",
                statusCode: StatusCodes.Status503ServiceUnavailable
            ),
            DownstreamApiException apiException
                when apiException.StatusCode == HttpStatusCode.NotFound => Results.NotFound(
                    apiException.ResponseBody
                ),
            DownstreamApiException apiException => Results.Problem(
                title: $"{apiException.ServiceName} returned an error.",
                detail: apiException.ResponseBody,
                statusCode: (int)apiException.StatusCode
            ),
            _ => Results.Problem(statusCode: StatusCodes.Status502BadGateway),
        };

        return exception is DownstreamUnavailableException or DownstreamApiException;
    }
}
