using System.Collections.Concurrent;
using MineSharp.Core;

namespace MineSharp.Commands;

public class CommandHandler
{
    public delegate Task<bool> CommandCallback(MinecraftServer server, MinecraftRemoteClient? remoteClient, params string[] args);

    private readonly ConcurrentDictionary<string, CommandCallback> _commands;

    public CommandHandler()
    {
        _commands = new ConcurrentDictionary<string, CommandCallback>();
    }

    public void RegisterCommand(string command, CommandCallback callback)
    {
        _commands.TryAdd(command, callback);
    }

    public async Task<bool> ParseAndExecuteCommandAsync(string completeCommand, MinecraftServer server, MinecraftRemoteClient? remoteClient)
    {
        if (!CommandParser.TryParse(completeCommand, out var parsedCommand))
            return false;

        if (!_commands.TryGetValue(parsedCommand.Command, out var handler))
            return false;

        return await handler(server, remoteClient, parsedCommand.Args);
    }
}