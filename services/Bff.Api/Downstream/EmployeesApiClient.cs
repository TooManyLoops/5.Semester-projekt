using System.Net;
using System.Net.Http.Json;
using Timegrip.Bff.Api.Downstream.Models;

namespace Timegrip.Bff.Api.Downstream;

public sealed class EmployeesApiClient(HttpClient httpClient)
{
    private const string ServiceName = "Employees.Api";

    public Task<List<EmployeeDto>> GetEmployees(CancellationToken cancellationToken) =>
        Send<List<EmployeeDto>>(HttpMethod.Get, "employees/", cancellationToken);

    public async Task<EmployeeDto?> GetEmployee(Guid employeeId, CancellationToken cancellationToken)
    {
        try
        {
            return await Send<EmployeeDto>(
                HttpMethod.Get,
                $"employees/{employeeId}",
                cancellationToken
            );
        }
        catch (DownstreamApiException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public Task<List<RoleDto>> GetRoles(CancellationToken cancellationToken) =>
        Send<List<RoleDto>>(HttpMethod.Get, "roles/", cancellationToken);

    public Task<List<EmployeeRoleDto>> GetEmployeeRoles(
        Guid employeeId,
        CancellationToken cancellationToken
    ) =>
        Send<List<EmployeeRoleDto>>(
            HttpMethod.Get,
            $"employee-roles/employee/{employeeId}",
            cancellationToken
        );

    public async Task<bool> EmployeeRoleExists(Guid employeeRoleId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"employee-roles/employeeRole/{employeeRoleId}"
        );
        using var response = await Send(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        await EnsureSuccess(response, cancellationToken);
        return true;
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
