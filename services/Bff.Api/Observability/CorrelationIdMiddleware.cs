namespace Timegrip.Bff.Api.Observability;

public sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task Invoke(
        HttpContext context,
        CorrelationContext correlationContext,
        ILogger<CorrelationIdMiddleware> logger
    )
    {
        var correlationId = GetOrCreateCorrelationId(context);
        correlationContext.SetCorrelationId(correlationId);
        context.Response.Headers[CorrelationContext.HeaderName] = correlationId;

        using (logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
        }))
        {
            await next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        var incomingCorrelationId = context.Request.Headers[CorrelationContext.HeaderName]
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(incomingCorrelationId)
            ? Guid.NewGuid().ToString("N")
            : incomingCorrelationId.Trim();
    }
}
