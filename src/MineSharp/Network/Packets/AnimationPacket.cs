using System.Buffers;
using MineSharp.Core.Packets;
using MineSharp.Extensions;

namespace MineSharp.Network.Packets;

public class AnimationPacket : IClientPacket, IServerPacket
{
    public const int Id = 0x12;

    public int EntityId { get; set; }
    public AnimationType Animation { get; set; }

    public void Read(ref SequenceReader<byte> reader)
    {
        EntityId = reader.ReadInt();
        Animation = (AnimationType) reader.ReadByte();
    }

    public void Write(PacketWriter writer)
    {
        writer.WriteByte(Id);
        writer.WriteInt(EntityId);
        writer.WriteByte((byte) Animation);
    }

    public enum AnimationType : byte
    {
        None = 0,
        SwingArm = 1,
        Damage = 2,
        LeaveBed = 3,
        Unknown = 102, //??
        Crouch = 104,
        Uncrouch = 105
    }
}