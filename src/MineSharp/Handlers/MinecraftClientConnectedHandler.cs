using Mediator;
using MineSharp.Notifications;

namespace MineSharp.Handlers;

public class MinecraftClientConnectedHandler : INotificationHandler<MinecraftClientConnected>
{
    public ValueTask Handle(MinecraftClientConnected command, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}