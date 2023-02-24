using System.Text.Json;
using System.Text.Json.Serialization;
using MineSharp.Core;

namespace MineSharp.ServerStatus;

public class ServerStatusResponse
{
    [JsonPropertyName("version")]
    public ServerStatusVersion? Version { get; set; }

    [JsonPropertyName("players")]
    public ServerStatusPlayers? Players { get; set; }

    [JsonPropertyName("description")]
    public Chat? Description { get; set; }

    [JsonPropertyName("favicon")]
    public string? Favicon { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}