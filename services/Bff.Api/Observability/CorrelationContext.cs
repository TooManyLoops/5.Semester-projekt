namespace Timegrip.Bff.Api.Observability;

public sealed class CorrelationContext
{
    public const string HeaderName = "X-Correlation-ID";

    public string CorrelationId { get; private set; } = string.Empty;

    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
