namespace CommanderD.Commands;

public class ExitCommand : AbstractCommand
{
    public ExitCommand(){}

    public CommandResult Execute(CommandContext context)
    {
        return CommandResult.Exit();
    }
}