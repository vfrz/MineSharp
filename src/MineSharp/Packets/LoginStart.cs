using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record LoginStart(MinecraftClient Client,
    string Name,
    bool HasPlayerUniqueId,
    Guid? PlayerUniqueId) : IPacket;