using Polly.Retry;

namespace Timegrip.Shifts.Api.Services;

public class VerificationService
{
    private readonly HttpClient _httpClient;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<VerificationService> _logger;

    public VerificationService(
        HttpClient httpClient,
        AsyncRetryPolicy retryPolicy,
        ILogger<VerificationService> logger)
    {
        _httpClient = httpClient;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }
    
    public async Task<bool> VerifyEmployeeRoleById(Guid employeeRoleId)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:8080/api/employee-roles/employeeRole/{employeeRoleId}");
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Response: {ResponseBody}", responseBody);
            });
            return true;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to verify employee {EmployeeRoleId}", employeeRoleId);
            return false;
        }
    }
}
