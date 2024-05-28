using MineSharp.Extensions;

namespace MineSharp.Commands;

public static class CommandParser
{
    public record ParsedCommand(string CommandName, string[] Args);

    public static ParsedCommand Parse(string command)
    {
        var commandCharArrayQueue = new Queue<char>(command.ToCharArray());

        //Dequeue '/'
        commandCharArrayQueue.Dequeue();

        var commandName = ParseCommandName(commandCharArrayQueue);
        var commandArguments = ParseCommandArguments(commandCharArrayQueue);

        return new ParsedCommand(commandName, commandArguments);
    }

    private static string ParseCommandName(Queue<char> queue)
    {
        var command = "";
        while (queue.IsNotEmpty() && queue.Peek() != ' ')
        {
            command += queue.Dequeue();
        }

        return command;
    }

    private static string[] ParseCommandArguments(Queue<char> queue)
    {
        var arguments = new List<string>();
        while (queue.IsNotEmpty())
        {
            var argument = ParseCommandArgument(queue);
            if (argument is null)
                break;
            arguments.Add(argument);
        }

        return arguments.ToArray();
    }

    private static string? ParseCommandArgument(Queue<char> queue)
    {
        while (queue.IsNotEmpty() && queue.Peek() == ' ')
        {
            queue.Dequeue();
        }

        if (queue.IsEmpty())
            return null;
        
        var argument = "";

        if (queue.Peek() == '"')
        {
            //Dequeue '"'
            queue.Dequeue();

            while (queue.IsNotEmpty())
            {
                var peek = queue.Peek();
                if (peek == '\\')
                {
                    queue.Dequeue();
                    if (queue.IsNotEmpty())
                    {
                        argument += queue.Dequeue();
                    }
                }
                else if (peek == '"')
                {
                    queue.Dequeue();
                    break;
                }
                else
                {
                    argument += queue.Dequeue();
                }
            }
        }
        else
        {
            while (queue.IsNotEmpty() && queue.Peek() != ' ')
            {
                argument += queue.Dequeue();
            }
        }

        return argument;
    }
}