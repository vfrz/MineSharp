using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record UnknownPacket(MinecraftClient Client, int PacketId, byte[] PacketData) : IPacket;