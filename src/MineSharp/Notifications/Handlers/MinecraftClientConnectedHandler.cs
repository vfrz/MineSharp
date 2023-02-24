using Mediator;

namespace MineSharp.Notifications.Handlers;

public class MinecraftClientConnectedHandler : INotificationHandler<MinecraftClientConnected>
{
    public ValueTask Handle(MinecraftClientConnected command, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}