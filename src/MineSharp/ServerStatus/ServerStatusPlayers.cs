using System.Text.Json.Serialization;

namespace MineSharp.ServerStatus;

public class ServerStatusPlayers
{
    [JsonPropertyName("max")]
    public int Max { get; set; }
    
    [JsonPropertyName("online")]
    public int Online { get; set; }
}