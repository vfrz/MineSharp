using MineSharp.Core;
using MineSharp.Plugins;

namespace TestPlugin;

public class TestPlugin : Plugin
{
    public TestPlugin(IServer server) : base(server)
    {
    }

    public override async Task OnPlayerJoinedAsync(IRemoteClient client)
    {
        await client.SendChatAsync($"Hello from {nameof(TestPlugin)}!");
    }
}