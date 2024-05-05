using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record ChatMessage(MinecraftClient Client, string Message) : IPacket;