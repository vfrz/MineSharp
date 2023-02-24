using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record ClientHandshake(MinecraftClient Client,
    int ProtocolVersion,
    string ServerAddress,
    int ServerPort, 
    MinecraftClientState NextState) : IPacket;