using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record StatusRequest(MinecraftClient Client) : IPacket;