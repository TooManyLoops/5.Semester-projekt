using System.Net;

namespace Timegrip.Gateway.Api.Downstream;

// Represents a reachable downstream service returning an unsuccessful HTTP status.
public sealed class DownstreamApiException(
    string serviceName,
    HttpStatusCode statusCode,
    string? responseBody = null
) : Exception($"{serviceName} returned {(int)statusCode}.")
{
    public string ServiceName { get; } = serviceName;
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string? ResponseBody { get; } = responseBody;
}
