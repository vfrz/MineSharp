using MineSharp.Core;
using MineSharp.Core.Packets;
using MineSharp.Network;

namespace MineSharp.Packets;

public record HandshakeRequest(MinecraftClient Client, string Username, MinecraftClientState NextState) : IPacket;