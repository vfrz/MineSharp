namespace MineSharp.Network;

public interface IServerPacket : IPacket
{
    void Write(PacketWriter writer);
}