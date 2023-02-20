using Mediator;
using MineSharp.Network;

namespace MineSharp.Commands;

public record ClientHandshake(MinecraftClient Client,
    int ProtocolVersion,
    string ServerAddress,
    int ServerPort,
    int NextState) : ICommand;