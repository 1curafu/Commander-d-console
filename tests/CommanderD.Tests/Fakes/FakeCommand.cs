using CommanderD.Commands;
using CommanderD.Shell;

namespace CommanderD.Tests.Fakes;

public class FakeCommand : AbstractCommand
{

    public CommandContext? LastContext { get; private set; }

    public FakeCommand(string name, IOutputWriter outputWriter)
        : base(name, "fake command", outputWriter)
    {
        
    }

    public override void Execute(CommandContext context)
    {
        OutputWriter.Write(Name);
        LastContext = context;
    }
}