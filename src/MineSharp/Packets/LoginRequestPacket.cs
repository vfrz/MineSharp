using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class LoginRequestPacket : IClientPacket
{
    public const int Id = 0x01;

    public int ProtocolVersion { get; private set; }
    public string Username { get; private set; }

    // Unused
    public long MapSeed { get; private set; }

    // Unused
    public sbyte Dimension { get; private set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        ProtocolVersion = reader.ReadInt();
        Username = reader.ReadString();
        MapSeed = reader.ReadLong();
        Dimension = reader.ReadSByte();
    }
}