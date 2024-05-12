using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class HoldingChangePacket : IClientPacket
{
    public const int Id = 0x10;
    public byte PacketId => Id;

    public short SlotId { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        SlotId = reader.ReadShort();
    }
}