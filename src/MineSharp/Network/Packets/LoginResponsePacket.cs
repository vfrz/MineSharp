using MineSharp.Core.Packets;
using MineSharp.World;

namespace MineSharp.Network.Packets;

public class LoginResponsePacket : IServerPacket
{
    public const int Id = 0x01;

    public byte PacketId => Id;

    public int EntityId { get; set; }

    // Unused by the client
    public long MapSeed { get; set; }
    public MinecraftDimension Dimension { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EntityId);
        writer.WriteString(string.Empty);
        writer.WriteLong(MapSeed);
        writer.WriteSByte((sbyte) Dimension);
    }
}