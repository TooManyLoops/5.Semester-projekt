using System.Net;
using System.Net.Http.Json;
using Timegrip.Gateway.Api.Downstream.Models;
using Timegrip.Gateway.Api.Downstream.Requests;
using Timegrip.Gateway.Api.Aggregation.Requests;

namespace Timegrip.Gateway.Api.Downstream;

public sealed class ShiftsApiClient(HttpClient httpClient)
{
    private const string ServiceName = "Shifts.Api";

    // These methods wrap Shifts.Api endpoints behind a small gateway-specific client.
    public Task<List<ShiftDto>> GetShifts(CancellationToken cancellationToken) =>
        Send<List<ShiftDto>>(HttpMethod.Get, "shifts/", cancellationToken);

    public Task<ShiftDto> CreateShift(
        CreateShiftDownstreamRequest request,
        CancellationToken cancellationToken
    ) =>
        Send<ShiftDto>(
            new HttpRequestMessage(HttpMethod.Post, "shifts/")
            {
                Content = JsonContent.Create(request, options: DownstreamJson.Options),
            },
            cancellationToken
        );

    public async Task AssignShift(
        Guid shiftId,
        AssignShiftGatewayRequest request,
        CancellationToken cancellationToken
    )
    {
        using var response = await Send(
            new HttpRequestMessage(HttpMethod.Post, "shifts/Assign/")
            {
                Content = JsonContent.Create(
                    new
                    {
                        ShiftId = shiftId,
                        request.EmployeeRoleId,
                        request.AssignmentStatus,
                    },
                    options: DownstreamJson.Options
                ),
            },
            cancellationToken
        );

        await EnsureSuccess(response, cancellationToken);
    }

    private async Task<T> Send<T>(
        HttpMethod method,
        string requestUri,
        CancellationToken cancellationToken
    )
    {
        // Deserialize successful downstream JSON into gateway-owned DTOs.
        using var request = new HttpRequestMessage(method, requestUri);
        using var response = await Send(request, cancellationToken);
        await EnsureSuccess(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<T>(
            DownstreamJson.Options,
            cancellationToken
        );

        return result
            ?? throw new DownstreamApiException(
                ServiceName,
                HttpStatusCode.BadGateway,
                "Downstream response body was empty."
            );
    }

    private async Task<T> Send<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        // Used by write operations where the request has a JSON body.
        using var response = await Send(request, cancellationToken);
        await EnsureSuccess(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<T>(
            DownstreamJson.Options,
            cancellationToken
        );

        return result
            ?? throw new DownstreamApiException(
                ServiceName,
                HttpStatusCode.BadGateway,
                "Downstream response body was empty."
            );
    }

    private static async Task EnsureSuccess(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        // Non-success responses become explicit gateway exceptions that endpoints can map consistently.
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        throw new DownstreamApiException(
            ServiceName,
            response.StatusCode,
            await response.Content.ReadAsStringAsync(cancellationToken)
        );
    }

    private async Task<HttpResponseMessage> Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await httpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception exception) when (
            exception is HttpRequestException or TaskCanceledException or TimeoutException
        )
        {
            // Connection failures and timeouts mean the downstream service is unavailable to the gateway.
            throw new DownstreamUnavailableException(ServiceName, exception);
        }
    }
}
