using System.Text.Json.Serialization;

namespace MineSharp.ServerStatus;

public class ServerStatusVersion
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("protocol")]
    public int Protocol { get; set; }
}