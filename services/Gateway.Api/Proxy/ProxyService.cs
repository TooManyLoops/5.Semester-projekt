using System.Net.Http.Headers;
using Timegrip.Gateway.Api.Aggregation.Services;

namespace Timegrip.Gateway.Api.Proxy;

public sealed class ProxyService(
    IHttpClientFactory httpClientFactory,
    RoleCatalogService roleCatalog
)
{
    // For simple CRUD, the gateway forwards requests without reshaping the response.
    public async Task ProxyRequest(
        HttpContext context,
        string clientName,
        string routePrefix,
        string? path
    )
    {
        using var requestMessage = CreateProxyRequest(context, routePrefix, path);
        using var responseMessage = await httpClientFactory
            .CreateClient(clientName)
            .SendAsync(
                requestMessage,
                HttpCompletionOption.ResponseHeadersRead,
                context.RequestAborted
            );

        context.Response.StatusCode = (int)responseMessage.StatusCode;

        // Role catalog cache must be refreshed after successful writes to the role resource.
        if (ShouldInvalidateRolesCache(context, routePrefix, responseMessage))
        {
            roleCatalog.InvalidateRoles();
        }

        foreach (var header in responseMessage.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        foreach (var header in responseMessage.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }

        context.Response.Headers.Remove("transfer-encoding");

        await responseMessage.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
    }

    private static bool ShouldInvalidateRolesCache(
        HttpContext context,
        string routePrefix,
        HttpResponseMessage responseMessage
    )
    {
        // Only mutating role requests can make the cached role catalog stale.
        return routePrefix.Equals("roles", StringComparison.OrdinalIgnoreCase)
            && responseMessage.IsSuccessStatusCode
            && (
                HttpMethods.IsPost(context.Request.Method)
                || HttpMethods.IsPut(context.Request.Method)
                || HttpMethods.IsPatch(context.Request.Method)
                || HttpMethods.IsDelete(context.Request.Method)
            );
    }

    private static HttpRequestMessage CreateProxyRequest(
        HttpContext context,
        string routePrefix,
        string? path
    )
    {
        // Rebuild the downstream request relative to the named HttpClient base address.
        var queryString = context.Request.QueryString.HasValue
            ? context.Request.QueryString.Value
            : string.Empty;
        var targetUri = $"{routePrefix}/{path}{queryString}";
        var requestMessage = new HttpRequestMessage(new HttpMethod(context.Request.Method), targetUri);

        if (
            HttpMethods.IsPost(context.Request.Method)
            || HttpMethods.IsPut(context.Request.Method)
            || HttpMethods.IsPatch(context.Request.Method)
        )
        {
            requestMessage.Content = new StreamContent(context.Request.Body);

            if (!string.IsNullOrWhiteSpace(context.Request.ContentType))
            {
                requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(
                    context.Request.ContentType
                );
            }
        }

        foreach (var header in context.Request.Headers)
        {
            // Hop-by-hop/body-specific headers are controlled by HttpClient and should not be copied blindly.
            if (
                header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase)
                || header.Key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
            )
            {
                continue;
            }

            if (!requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
            {
                requestMessage.Content?.Headers.TryAddWithoutValidation(
                    header.Key,
                    header.Value.ToArray()
                );
            }
        }

        requestMessage.Headers.Host = null;
        return requestMessage;
    }
}
