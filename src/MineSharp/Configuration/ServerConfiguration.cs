namespace MineSharp.Configuration;

public class ServerConfiguration
{
    public string Description { get; set; } = null!;

    public int MaxPlayers { get; set; }
    
    public int Port { get; set; }
}