using Mediator;
using MineSharp.Network;

namespace MineSharp.Commands;

public record PingRequest(MinecraftClient Client, long Payload) : ICommand;
