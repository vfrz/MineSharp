using MineSharp.Core;
using MineSharp.Items;

namespace MineSharp.Entities;

public class PickupItem(MinecraftServer server, TimeSpan expiration) : Entity(server)
{
    private readonly long _creationTimestamp = TimeProvider.System.GetTimestamp();

    public bool IsExpired => TimeProvider.System.GetElapsedTime(_creationTimestamp) >= expiration;

    public ItemId ItemId { get; set; }

    public byte Count { get; set; }

    public short PickupMetadata { get; set; }

    public int AbsoluteX { get; set; }

    public int AbsoluteY { get; set; }

    public int AbsoluteZ { get; set; }

    public byte Rotation { get; set; }

    public byte Pitch { get; set; }

    public byte Roll { get; set; }
}