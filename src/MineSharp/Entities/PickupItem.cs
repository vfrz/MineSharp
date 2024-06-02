using MineSharp.Content;
using MineSharp.Core;

namespace MineSharp.Entities;

public class PickupItem(MinecraftServer server, TimeSpan expiration) : Entity(server)
{
    private readonly long _creationTimestamp = TimeProvider.System.GetTimestamp();

    public bool IsExpired => TimeProvider.System.GetElapsedTime(_creationTimestamp) >= expiration;

    public ItemStack Item { get; set; }

    public Vector3<int> AbsolutePosition { get; set; }

    public byte Rotation { get; set; }

    public byte Pitch { get; set; }

    public byte Roll { get; set; }
}