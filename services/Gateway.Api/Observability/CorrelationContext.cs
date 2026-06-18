namespace Timegrip.Gateway.Api.Observability;

public sealed class CorrelationContext
{
    public const string HeaderName = "X-Correlation-ID";

    // Scoped storage for the current gateway request's correlation id.
    public string CorrelationId { get; private set; } = string.Empty;

    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
