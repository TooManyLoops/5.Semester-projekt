using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Timegrip.Bff.Api.Observability;

namespace Bff.Api.Tests;

public sealed class ObservabilityTests
{
    [Fact]
    public async Task CorrelationIdMiddleware_WhenRequestHasCorrelationId_ShouldExposeItOnResponse()
    {
        const string correlationId = "test-correlation-id";
        var context = new DefaultHttpContext();
        context.Request.Headers[CorrelationContext.HeaderName] = correlationId;
        context.Response.Body = new MemoryStream();
        var correlationContext = new CorrelationContext();
        var middleware = new CorrelationIdMiddleware(_ => Task.CompletedTask);

        await middleware.Invoke(
            context,
            correlationContext,
            NullLogger<CorrelationIdMiddleware>.Instance
        );
        await context.Response.StartAsync();

        correlationContext.CorrelationId.Should().Be(correlationId);
        context.Response.Headers[CorrelationContext.HeaderName].ToString().Should().Be(correlationId);
    }

    [Fact]
    public async Task DownstreamLoggingHandler_ShouldForwardCorrelationIdHeader()
    {
        const string correlationId = "gateway-request-123";
        var correlationContext = new CorrelationContext();
        correlationContext.SetCorrelationId(correlationId);
        var captureHandler = new CaptureHttpMessageHandler();
        var handler = new DownstreamLoggingHandler(
            correlationContext,
            NullLogger<DownstreamLoggingHandler>.Instance
        )
        {
            InnerHandler = captureHandler,
        };
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://downstream.test"),
        };

        await httpClient.GetAsync("roles/");

        captureHandler.LastRequest.Should().NotBeNull();
        captureHandler.LastRequest!.Headers
            .GetValues(CorrelationContext.HeaderName)
            .Should()
            .ContainSingle(correlationId);
    }

    private sealed class CaptureHttpMessageHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
