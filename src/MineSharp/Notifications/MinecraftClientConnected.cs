using Mediator;
using MineSharp.Network;

namespace MineSharp.Notifications;

public record MinecraftClientConnected(MinecraftClient Client) : INotification;