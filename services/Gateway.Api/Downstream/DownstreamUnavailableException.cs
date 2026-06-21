namespace Timegrip.Gateway.Api.Downstream;

// Represents a downstream service the gateway could not reach, such as timeout or connection failure.
public sealed class DownstreamUnavailableException(string serviceName, Exception innerException)
    : Exception($"{serviceName} is unavailable.", innerException)
{
    public string ServiceName { get; } = serviceName;
}
