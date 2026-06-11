namespace Timegrip.Shifts.Api.Services;

public class VerificationService(IHttpClientFactory httpClientFactory)
{
    private readonly HttpClient _client = httpClientFactory.CreateClient("ResilientClient");

    public async Task<string> VerifyAsync(Guid employeeRoleId,CancellationToken ct = default)
    {
        var response = await _client.GetAsync($"https://localhost:5000/employees/employeeRole/{employeeRoleId:guid}", ct);
        return await response.Content.ReadAsStringAsync(ct);
    }
}