using CommanderD.Shell;

namespace CommanderD.Commands;

public class HelpCommand : Command
{
    private const string NotSupportedMessage = "This command is not supported by the help utility.";
    private const int NameWidth = 10;

    private readonly CommandFactory _factory;

    public HelpCommand(CommandFactory factory, IOutputWriter outputWriter)
        : base("help", "Shows all commands, or details for one command.", outputWriter)
    {
        _factory = factory;
    }

    public override string Usage => "HELP\nHELP <command>";

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            ShowAllCommands();
            return;
        }

        ShowCommand(context.Arguments[0].Trim('"'));
    }

    private void ShowAllCommands()
    {
        OutputWriter.WriteLn("For more information on a specific command, type HELP <command>.");
        OutputWriter.WriteLn(string.Empty);

        var shown = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var commands = new List<Command>();

        foreach (string name in _factory.GetCommands())
        {
            Command? command = _factory.Create(name, OutputWriter);

            if (command != null && shown.Add(command.Name))
            {
                commands.Add(command);
            }
        }

        foreach (Command command in commands.OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase))
        {
            OutputWriter.WriteLn($"{command.Name.ToUpperInvariant().PadRight(NameWidth)}{command.Description}");
        }
    }

    private void ShowCommand(string name)
    {
        Command? command = _factory.Create(name, OutputWriter);

        if (command == null)
        {
            OutputWriter.WriteLn(NotSupportedMessage);
            return;
        }

        OutputWriter.WriteLn(command.Description);
        OutputWriter.WriteLn(string.Empty);
        OutputWriter.WriteLn(command.Usage);
    }
}
