using System.Net;

namespace Timegrip.Bff.Api.Downstream;

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
