using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record PlayerBlockPlacement(MinecraftClient Client,
    int X, sbyte Y, int Z, sbyte Direction, short BlockId, byte? Amount, short? Damage) : IPacket;
