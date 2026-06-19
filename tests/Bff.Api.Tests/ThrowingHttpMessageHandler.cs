namespace Bff.Api.Tests;

internal sealed class ThrowingHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        throw new HttpRequestException("Downstream unavailable");
    }
}
