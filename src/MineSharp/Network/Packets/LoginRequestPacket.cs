using System.Buffers;
using MineSharp.Extensions;
using MineSharp.World;

namespace MineSharp.Network.Packets;

public class LoginRequestPacket : IClientPacket
{
    public const int Id = 0x01;
    public byte PacketId => Id;

    public int ProtocolVersion { get; set; }
    public string Username { get; set; } = string.Empty;
    public long MapSeed { get; set; } // Unused
    public MinecraftDimension Dimension { get; set; } // Unused

    public void Read(ref SequenceReader<byte> reader)
    {
        ProtocolVersion = reader.ReadInt();
        Username = reader.ReadString();
        MapSeed = reader.ReadLong();
        Dimension = (MinecraftDimension) reader.ReadSByte();
    }
}