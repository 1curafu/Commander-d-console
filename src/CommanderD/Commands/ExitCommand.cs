namespace CommanderD.Commands;

public class ExitCommand : AbstractCommand
{

    public ExitCommand()
        :base("exit", "exits the shell")
    {
        
    }
    public override CommandResult Execute(CommandContext context)
    {
        return CommandResult.Exit();
    }
}