using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record LoginRequest(MinecraftClient Client,
    int ProtocolVersion,
    string Username) : IPacket;