using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Packets;

public class PlayerPacket : IClientPacket
{
    public const int Id = 0x0A;
    
    public bool OnGround { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        OnGround = reader.ReadBool();
    }
}