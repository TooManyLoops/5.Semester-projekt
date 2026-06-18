using System.Text.Json;

namespace Timegrip.Gateway.Api.Downstream;

public static class DownstreamJson
{
    // Shared JSON settings for all gateway-to-service HTTP serialization.
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };
}
