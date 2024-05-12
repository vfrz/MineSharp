using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class SoundEffectPacket : IServerPacket
{
    public const byte Id = 0x3D;
    public byte PacketId => Id;

    public int EffectId { get; set; }
    public int X { get; set; }
    public byte Y { get; set; }
    public int Z { get; set; }
    public int SoundData { get; set; }

    public void Write(PacketWriter writer)
    {
        writer.WriteInt(EffectId);
        writer.WriteInt(X);
        writer.WriteByte(Y);
        writer.WriteInt(Z);
        writer.WriteInt(SoundData);
    }
}