using System.Diagnostics;

namespace Timegrip.Bff.Api.Observability;

public sealed class DownstreamLoggingHandler(
    CorrelationContext correlationContext,
    ILogger<DownstreamLoggingHandler> logger
) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        if (!string.IsNullOrWhiteSpace(correlationContext.CorrelationId))
        {
            request.Headers.Remove(CorrelationContext.HeaderName);
            request.Headers.TryAddWithoutValidation(
                CorrelationContext.HeaderName,
                correlationContext.CorrelationId
            );
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await base.SendAsync(request, cancellationToken);
            stopwatch.Stop();

            logger.LogInformation(
                "Downstream call {Method} {Uri} responded {StatusCode} in {ElapsedMilliseconds} ms. CorrelationId={CorrelationId}",
                request.Method.Method,
                request.RequestUri,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                correlationContext.CorrelationId
            );

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            logger.LogWarning(
                exception,
                "Downstream call {Method} {Uri} failed in {ElapsedMilliseconds} ms. CorrelationId={CorrelationId}",
                request.Method.Method,
                request.RequestUri,
                stopwatch.ElapsedMilliseconds,
                correlationContext.CorrelationId
            );

            throw;
        }
    }
}
