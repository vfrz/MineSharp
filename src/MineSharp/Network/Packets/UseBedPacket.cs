using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class UseBedPacket : IServerPacket
{
    public const byte Id = 0x11;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public byte InBed { get; set; } //TODO Not really sure of this flag
    public int X { get; set; }
    public byte Y { get; set; }
    public int Z { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteByte(InBed);
        writer.WriteInt(X);
        writer.WriteByte(Y);
        writer.WriteInt(Z);
    }
}