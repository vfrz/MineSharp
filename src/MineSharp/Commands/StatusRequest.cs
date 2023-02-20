using Mediator;
using MineSharp.Network;

namespace MineSharp.Commands;

public record StatusRequest(MinecraftClient Client) : ICommand;