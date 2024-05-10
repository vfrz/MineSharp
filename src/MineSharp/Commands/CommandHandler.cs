using System.Collections.Concurrent;
using MineSharp.Core;

namespace MineSharp.Commands;

public class CommandHandler
{
    public delegate Task<bool> CommandCallback(MinecraftServer server, MinecraftRemoteClient? remoteClient, params string[] args);

    private readonly ConcurrentDictionary<string, CommandCallback> _commands = new();

    public bool TryRegisterCommand(string command, CommandCallback callback)
    {
        return _commands.TryAdd(command, callback);
    }

    public async Task<bool> TryExecuteCommandAsync(string completeCommand, MinecraftServer server, MinecraftRemoteClient? remoteClient)
    {
        if (!CommandParser.TryParse(completeCommand, out var parsedCommand))
        {
            if (remoteClient is not null)
                await remoteClient.SendChatAsync("Failed to read the command, please be sure to use it correctly.");
            return false;
        }

        if (!_commands.TryGetValue(parsedCommand.Command, out var handler))
        {
            if (remoteClient is not null)
                await remoteClient.SendChatAsync("Command not found.");
            return false;
        }

        return await handler(server, remoteClient, parsedCommand.Args);
    }
}