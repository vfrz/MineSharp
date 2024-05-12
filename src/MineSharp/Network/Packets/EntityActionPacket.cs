using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class EntityActionPacket : IClientPacket
{
    public const int Id = 0x13;
    public byte PacketId => Id;

    public int EntityId { get; set; }
    public ActionType Action { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        EntityId = reader.ReadInt();
        Action = (ActionType) reader.ReadByte();
    }

    public enum ActionType : byte
    {
        Crouch = 1,
        Uncrouch = 2,
        LeaveBed = 3
    }
}