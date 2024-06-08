using MineSharp.Content;
using MineSharp.Entities;
using MineSharp.Sdk.Core;

namespace MineSharp.Network.Packets;

public class PickupSpawnPacket : IServerPacket
{
    public const int Id = 0x15;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public ItemId ItemId { get; set; }
    public byte Count { get; set; }
    public short Metadata { get; set; }
    public int AbsoluteX { get; set; }
    public int AbsoluteY { get; set; }
    public int AbsoluteZ { get; set; }
    public byte Rotation { get; set; }
    public byte Pitch { get; set; }
    public byte Roll { get; set; }

    public PickupSpawnPacket(PickupItem? pickupItem = null)
    {
        if (pickupItem is not null)
        {
            EntityId = pickupItem.EntityId;
            ItemId = pickupItem.Item.ItemId;
            Count = pickupItem.Item.Count;
            Metadata = pickupItem.Item.Metadata;
            AbsoluteX = pickupItem.AbsolutePosition.X;
            AbsoluteY = pickupItem.AbsolutePosition.Y;
            AbsoluteZ = pickupItem.AbsolutePosition.Z;
            Rotation = pickupItem.Rotation;
            Pitch = pickupItem.Pitch;
            Roll = pickupItem.Roll;
        }
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteShort((short)ItemId);
        writer.WriteByte(Count);
        writer.WriteShort(Metadata);
        writer.WriteInt(AbsoluteX);
        writer.WriteInt(AbsoluteY);
        writer.WriteInt(AbsoluteZ);
        writer.WriteByte(Rotation);
        writer.WriteByte(Pitch);
        writer.WriteByte(Roll);
    }
}