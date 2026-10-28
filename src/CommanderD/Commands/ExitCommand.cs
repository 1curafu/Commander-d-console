using CommanderD.Shell;

namespace CommanderD.Commands;

public class ExitCommand : AbstractCommand
{

    public ExitCommand()
        :base("exit", "exits the shell")
    {
        
    }
    public override void Execute(CommandContext context)
    {
        Environment.Exit(0);
    }
}