using System.Net;
using System.Net.Http.Json;
using Timegrip.Bff.Api.Downstream.Models;
using Timegrip.Bff.Api.Gateway.Requests;

namespace Timegrip.Bff.Api.Downstream;

public sealed class ShiftsApiClient(HttpClient httpClient)
{
    private const string ServiceName = "Shifts.Api";

    public Task<List<ShiftDto>> GetShifts(CancellationToken cancellationToken) =>
        Send<List<ShiftDto>>(HttpMethod.Get, "shifts/", cancellationToken);

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

    private static async Task EnsureSuccess(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
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
            throw new DownstreamUnavailableException(ServiceName, exception);
        }
    }
}
