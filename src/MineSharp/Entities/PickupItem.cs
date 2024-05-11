namespace MineSharp.Entities;

public class PickupItem : Entity
{
    private readonly TimeSpan _expiration;
    private readonly long _creationTimestamp;

    public bool IsExpired => TimeProvider.System.GetElapsedTime(_creationTimestamp) >= _expiration;

    public short ItemId { get; set; }

    public byte Count { get; set; }

    public short Metadata { get; set; }

    public int AbsoluteX { get; set; }

    public int AbsoluteY { get; set; }

    public int AbsoluteZ { get; set; }

    public byte Rotation { get; set; }

    public byte Pitch { get; set; }

    public byte Roll { get; set; }

    public PickupItem(TimeSpan expiration)
    {
        _expiration = expiration;
        _creationTimestamp = TimeProvider.System.GetTimestamp();
    }
}