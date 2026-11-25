using System.Text.Json;

namespace Insurance.Tests.Integration;

public static class TestJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
