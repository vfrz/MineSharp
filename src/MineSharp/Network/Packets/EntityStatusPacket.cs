using MineSharp.Core.Packets;
using MineSharp.Entities;

namespace MineSharp.Network.Packets;

public class EntityStatusPacket : IServerPacket
{
    public const int Id = 0x26;

    public byte PacketId => Id;

    public int EntityId { get; set; }

    public EntityStatus Status { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteByte((byte) Status);
    }
}