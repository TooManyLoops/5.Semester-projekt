using System.Text.Json;

namespace Timegrip.Bff.Api.Downstream;

public static class DownstreamJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };
}
