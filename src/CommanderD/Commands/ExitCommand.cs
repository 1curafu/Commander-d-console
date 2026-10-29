using CommanderD.Shell;

namespace CommanderD.Commands;

public class ExitCommand : AbstractCommand
{

    public ExitCommand(IOutputWriter? outputWriter)
        :base("exit", "exits the shell",  outputWriter)
    {
        
    }
    public override void Execute(CommandContext context)
    {
        Environment.Exit(0);
    }
}