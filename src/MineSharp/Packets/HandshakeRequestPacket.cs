using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class HandshakeRequestPacket : IClientPacket
{
    public const int Id = 0x02;
    
    public string Username { get; private set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        Username = reader.ReadString();
    }
}