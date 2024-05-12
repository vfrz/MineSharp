using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class AttachEntityPacket : IServerPacket
{
    public const byte Id = 0x27;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public int VehicleEntityId { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteInt(VehicleEntityId);
    }
}