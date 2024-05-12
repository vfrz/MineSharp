using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class EntityPacket : IServerPacket
{
    public const byte Id = 0x1E;
    public byte PacketId => Id;

    public int EntityId { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
    }
}