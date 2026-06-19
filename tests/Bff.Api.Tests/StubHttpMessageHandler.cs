using System.Net;
using System.Net.Http.Json;

namespace Bff.Api.Tests;

internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _routes = [];

    public List<HttpRequestMessage> Requests { get; } = [];

    public StubHttpMessageHandler MapJson(string path, object response)
    {
        _routes[path] = _ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(response),
        };

        return this;
    }

    public StubHttpMessageHandler MapStatus(string path, HttpStatusCode statusCode)
    {
        _routes[path] = _ => new HttpResponseMessage(statusCode);
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        Requests.Add(request);

        if (_routes.TryGetValue(request.RequestUri?.AbsolutePath ?? string.Empty, out var route))
        {
            return Task.FromResult(route(request));
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
    }
}
