using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Timegrip.Gateway.Api.Downstream.Models;
using Timegrip.Gateway.Api.Downstream.Responses;

namespace Timegrip.Gateway.Api.Downstream;

public sealed class ImportApiClient(HttpClient httpClient)
{
    private const string ServiceName = "Import.Api";

    public async Task<ShiftImportParseResponse> ParseShifts(
        IFormFile file,
        IReadOnlyCollection<RoleDto> roles,
        CancellationToken cancellationToken
    )
    {
        using var content = new MultipartFormDataContent();
        await using var fileStream = file.OpenReadStream();
        using var fileContent = new StreamContent(fileStream);

        if (!string.IsNullOrWhiteSpace(file.ContentType))
        {
            fileContent.Headers.ContentType = new(file.ContentType);
        }

        content.Add(fileContent, "file", file.FileName);
        content.Add(
            new StringContent(
                JsonSerializer.Serialize(roles, DownstreamJson.Options),
                Encoding.UTF8,
                "application/json"
            ),
            "roles"
        );

        using var response = await Send(
            new HttpRequestMessage(HttpMethod.Post, "imports/shifts/parse")
            {
                Content = content,
            },
            cancellationToken
        );

        var result = await response.Content.ReadFromJsonAsync<ShiftImportParseResponse>(
            DownstreamJson.Options,
            cancellationToken
        );

        if (response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest)
        {
            return result
                ?? throw new DownstreamApiException(
                    ServiceName,
                    HttpStatusCode.BadGateway,
                    "Downstream response body was empty."
                );
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
