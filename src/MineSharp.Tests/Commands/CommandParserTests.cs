using FluentAssertions;
using MineSharp.Commands;

namespace MineSharp.Tests.Commands;

[TestClass]
public class CommandParserTests
{
    [TestMethod]
    public void CommandParser_CommandWithoutArgument()
    {
        var command = "/test";

        var parsedCommand = CommandParser.Parse(command);

        parsedCommand.CommandName.Should().Be("test");
        parsedCommand.Args.Should().BeEmpty();
    }

    [TestMethod]
    public void CommandParser_CommandWithOneSimpleArgument()
    {
        var command = "/hello world";

        var parsedCommand = CommandParser.Parse(command);

        parsedCommand.CommandName.Should().Be("hello");
        parsedCommand.Args.Should().HaveCount(1);
        parsedCommand.Args[0].Should().Be("world");
    }
    
    [TestMethod]
    public void CommandParser_CommandWithThreeArguments()
    {
        var command = "/hello world lorem ipsum";

        var parsedCommand = CommandParser.Parse(command);

        parsedCommand.CommandName.Should().Be("hello");
        parsedCommand.Args.Should().HaveCount(3);
        parsedCommand.Args[0].Should().Be("world");
        parsedCommand.Args[1].Should().Be("lorem");
        parsedCommand.Args[2].Should().Be("ipsum");
    }

    [TestMethod]
    public void CommandParser_CommandWithOneStringArgument()
    {
        var command = "/kick \"Bad player\"";

        var parsedCommand = CommandParser.Parse(command);

        parsedCommand.CommandName.Should().Be("kick");
        parsedCommand.Args.Should().HaveCount(1);
        parsedCommand.Args[0].Should().Be("Bad player");
    }
    
    [TestMethod]
    public void CommandParser_CommandWithOneStringArgumentWithEscapedQuote()
    {
        var command = "/kick \"Bad \\\"player\\\"\"";

        var parsedCommand = CommandParser.Parse(command);

        parsedCommand.CommandName.Should().Be("kick");
        parsedCommand.Args.Should().HaveCount(1);
        parsedCommand.Args[0].Should().Be("Bad \"player\"");
    }
    
    [TestMethod]
    public void CommandParser_CommandWithMultipleMixedArgumentsAndTooManySpaces()
    {
        var command = "/kick   the  \"Bad \\\"player\\\"\"  now ";

        var parsedCommand = CommandParser.Parse(command);

        parsedCommand.CommandName.Should().Be("kick");
        parsedCommand.Args.Should().HaveCount(3);
        parsedCommand.Args[0].Should().Be("the");
        parsedCommand.Args[1].Should().Be("Bad \"player\"");
        parsedCommand.Args[2].Should().Be("now");
    }
}