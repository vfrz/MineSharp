using Mediator;
using MineSharp.Network;

namespace MineSharp.Commands;

public record UnknownPacket(MinecraftClient Client, int PacketId, byte[] PacketData) : ICommand;