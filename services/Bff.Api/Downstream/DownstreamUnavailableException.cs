namespace Timegrip.Bff.Api.Downstream;

public sealed class DownstreamUnavailableException(string serviceName, Exception innerException)
    : Exception($"{serviceName} is unavailable.", innerException)
{
    public string ServiceName { get; } = serviceName;
}
