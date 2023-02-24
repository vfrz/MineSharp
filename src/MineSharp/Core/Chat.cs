using System.Text.Json;
using System.Text.Json.Serialization;

namespace MineSharp.Core;

public class Chat
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
    
    [JsonPropertyName("bold")]
    public bool? Bold { get; set; }
    
    [JsonPropertyName("italic")]
    public bool? Italic { get; set; }
    
    [JsonPropertyName("strikethrough")]
    public bool? StrikeThrough { get; set; }
    
    [JsonPropertyName("obfuscated")]
    public bool? Obfuscated { get; set; }
    
    [JsonPropertyName("font")]
    public string? Font { get; set; }
    
    [JsonPropertyName("insertion")]
    public string? Insertion { get; set; }

    public Chat(string text)
    {
        Text = text;
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions(JsonSerializerDefaults.General)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}