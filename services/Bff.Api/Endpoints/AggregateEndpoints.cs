using System.Net;
using Timegrip.Bff.Api.Downstream;
using Timegrip.Bff.Api.Gateway.Requests;
using Timegrip.Bff.Api.Gateway.Services;

namespace Timegrip.Bff.Api.Endpoints;

public static class AggregateEndpoints
{
    public static WebApplication MapAggregateEndpoints(this WebApplication app)
    {
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
