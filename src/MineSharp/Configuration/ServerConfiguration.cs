using System.ComponentModel.DataAnnotations;

namespace MineSharp.Configuration;

public class ServerConfiguration
{
    public int MaxPlayers { get; set; } = 10;

    public int Port { get; set; } = 25565;

    [Range(3, 12)]
    public int VisibleChunksDistance { get; set; } = 6;

    public bool Debug { get; set; }
}