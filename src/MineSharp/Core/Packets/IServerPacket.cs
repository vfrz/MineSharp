namespace MineSharp.Core.Packets;

public interface IServerPacket : IPacket
{
    void Write(PacketWriter writer);
}