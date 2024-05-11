using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class CollectItemPacket : IServerPacket
{
    public const int Id = 0x16;
    public byte PacketId => Id;

    public int CollectedEntityId { get; set; }

    public int CollectorEntityId { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(CollectedEntityId);
        writer.WriteInt(CollectorEntityId);
    }
}