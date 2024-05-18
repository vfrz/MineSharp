using System.ComponentModel.DataAnnotations;

namespace MineSharp.Configuration;

public class ServerConfiguration
{
    [Range(1, 100)]
    public int MaxPlayers { get; set; } = 10;

    public int Port { get; set; } = 25565;

    [Range(3, 64)]
    public int VisibleChunksDistance { get; set; } = 12;

    public double AutomaticSaveIntervalInMinutes { get; set; } = 1;

    public int? WorldSeed { get; set; }

    public bool Debug { get; set; }
}