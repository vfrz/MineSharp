using System.Text.Json.Serialization;

namespace MineSharp.ServerStatus;

public class ServerStatusResponse
{
    [JsonPropertyName("version")]
    public ServerStatusVersion? Version { get; set; }
    
    [JsonPropertyName("players")]
    public ServerStatusPlayers? Players { get; set; }
    
    [JsonPropertyName("description")]
    public ServerStatusDescription? Description { get; set; }
    
    [JsonPropertyName("favicon")]
    public string? Favicon { get; set; }
}