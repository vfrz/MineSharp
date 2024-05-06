using MineSharp.Core.Packets;

namespace MineSharp.Packets;

public class LoginResponsePacket : IServerPacket
{
    public const int Id = 0x01;

    public int EntityId { get; set; }

    // Unused by the client
    public long MapSeed { get; set; }
    public sbyte Dimension { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        writer.WriteString(string.Empty);
        writer.WriteLong(MapSeed);
        writer.WriteSByte(Dimension);
    }
}