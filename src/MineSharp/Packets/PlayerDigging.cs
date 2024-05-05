using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record PlayerDigging(MinecraftClient Client, PlayerDiggingStatus Status, int X, sbyte Y, int Z, sbyte Face) : IPacket;