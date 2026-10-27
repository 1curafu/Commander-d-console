using CommanderD.Commands;

namespace CommanderD.Shell;

public class CommandInvoker
{
    private readonly CommandParser parser;
    private readonly CommandFactory factory;

    public CommandInvoker(CommandParser parser, CommandFactory factory)
    {
        this.parser = parser;
        this.factory = factory;
    }

    public CommandResult ExecuteCommand(string input)
    {
        CommandContext? context = parser.Parse(input);

        if (context == null)
        {
            return CommandResult.Ok();
        }

        ICommand? command = factory.Create(context.Name);

        if (command == null)
        {
            return CommandResult.Fail(
                $"'{context.Name}' is not recognized as an internal or external command, operable program or batch file.");
        }

        return command.Execute(context);
    }
}