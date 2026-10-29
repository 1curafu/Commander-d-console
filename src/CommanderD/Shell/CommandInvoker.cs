using CommanderD.Commands;

namespace CommanderD.Shell;

public class CommandInvoker
{
    private readonly CommandParser _parser;
    private readonly CommandFactory _factory;
    public readonly IOutputWriter OutputWriter;

    public CommandInvoker(CommandParser parser, CommandFactory factory, IOutputWriter outputWriter)
    {
        this._parser = parser;
        this._factory = factory;
        this.OutputWriter = outputWriter;
    }

    public AbstractCommand? ExecuteCommand(string input)
    {
        CommandContext? context = _parser.Parse(input);

        if (context == null)
        {
            return null;
        }

        AbstractCommand? command = _factory.Create(context.Name, OutputWriter);

        if (command == null)
        {
            OutputWriter.WriteLn($"'{context.Name}' is not recognized as an internal or external command, operable program or batch file.");
            return null;
        }

        command.Execute(context);
        return command;
    }
}