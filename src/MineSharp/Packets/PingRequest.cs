using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record PingRequest(MinecraftClient Client, long Payload) : IPacket;
