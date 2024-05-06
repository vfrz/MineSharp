using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class LoginRequestPacket : IClientPacket
{
    public const int Id = 0x01;

    public int ProtocolVersion { get; set; }
    public string Username { get; set; }

    // Unused
    public long MapSeed { get; set; }

    // Unused
    public sbyte Dimension { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        ProtocolVersion = reader.ReadInt();
        Username = reader.ReadString();
        MapSeed = reader.ReadLong();
        Dimension = reader.ReadSByte();
    }
}