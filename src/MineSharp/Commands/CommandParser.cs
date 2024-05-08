using System.Text.RegularExpressions;

namespace MineSharp.Commands;

public static partial class CommandParser
{
    [GeneratedRegex("\\/(?:[^\\s\"\"]+)|(?:[^\\s\"\"]+|\"\"(?:[^\"\"\\\\]|\\\\.)*\"\")+|(?:\"(?:[^\"\\\\]|\\\\.)*\\\\?\")|(?:\"(?:[^\"\\\\]|\\\\.)*\")")]
    private static partial Regex GenerateCommandRegex();

    public record Result(string Command, string[] Args);

    public static bool TryParse(string command, out Result result)
    {
        var matchResult = GenerateCommandRegex().Matches(command);

        if (!matchResult.Any())
        {
            result = new Result(string.Empty, []);
            return false;
        }

        var commandValue = matchResult.First().Value[1..];
        var args = matchResult.Skip(1).Select(match =>
        {
            if (match.Value.StartsWith('"') && match.Value.EndsWith('"'))
                return match.Value.Substring(1, match.Value.Length - 2).Replace("\\\"", "\"");
            return match.Value;
        }).ToArray();
        result = new Result(commandValue, args);
        return true;
    }
}