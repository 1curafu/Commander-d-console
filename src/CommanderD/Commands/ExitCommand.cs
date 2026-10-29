using CommanderD.Shell;

namespace CommanderD.Commands;

public class ExitCommand : AbstractCommand
{
    public bool ShouldExit { get; private set; }

    public ExitCommand(IOutputWriter? outputWriter)
        : base("exit", "exits the shell", outputWriter)
    {
    }

    public override void Execute(CommandContext context)
    {
        ShouldExit = true;
    }
}
