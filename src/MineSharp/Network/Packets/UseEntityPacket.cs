using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class UseEntityPacket : IClientPacket
{
    public const int Id = 0x07;

    public byte PacketId => Id;

    public int PlayerEntityId { get; set; }

    public int TargetEntityId { get; set; }

    public bool LeftClick { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        PlayerEntityId = reader.ReadInt();
        TargetEntityId = reader.ReadInt();
        LeftClick = reader.ReadBool();
    }
}