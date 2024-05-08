using MineSharp.Core.Packets;

namespace MineSharp.Network.Packets;

public class UpdateHealthPacket : IServerPacket
{
    public const int Id = 0x08;
    
    public short Health { get; set; }
    
    public void Write(PacketWriter writer)
    {
        writer.WriteInt(Id);
        writer.WriteShort(Health);
    }
}