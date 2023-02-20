using System.Text.Json.Serialization;

namespace MineSharp.ServerStatus;

public class ServerStatusDescription
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
}