using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class HandshakeRequestPacket : IClientPacket
{
    public const int Id = 0x02;

    public byte PacketId => Id;

    public string Username { get; set; } = string.Empty;

    public void Read(ref SequenceReader<byte> reader)
    {
        Username = reader.ReadString();
    }
}