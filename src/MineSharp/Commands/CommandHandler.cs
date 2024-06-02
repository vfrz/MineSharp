using System.Collections.Concurrent;
using MineSharp.Core;

namespace MineSharp.Commands;

public class CommandHandler
{
    public delegate Task<bool> CommandCallback(MinecraftServer server, RemoteClient? remoteClient, params string[] args);

    private readonly ConcurrentDictionary<string, CommandCallback> _commands = new();

    public bool TryRegisterCommand(string command, CommandCallback callback)
    {
        return _commands.TryAdd(command, callback);
    }

    public async Task<bool> TryExecuteCommandAsync(string completeCommand, MinecraftServer server, RemoteClient? remoteClient)
    {
        var parsedCommand = CommandParser.Parse(completeCommand);

        if (!_commands.TryGetValue(parsedCommand.CommandName, out var handler))
        {
            if (remoteClient is not null)
                await remoteClient.SendChatAsync($"{ChatColors.Red}Command not found: {parsedCommand.CommandName}");
            return false;
        }

        try
        {
            return await handler(server, remoteClient, parsedCommand.Args);
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to execute command: '{completeCommand}'", ex);
        }
    }
}