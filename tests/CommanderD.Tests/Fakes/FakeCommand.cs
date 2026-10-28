using CommanderD.Commands;
using CommanderD.Shell;

namespace CommanderD.Tests.Fakes;

public class FakeCommand : AbstractCommand
{

    public CommandContext? LastContext { get; private set; }

    public FakeCommand(string name = "fake")
        : base(name, "fake command", new TestOutputWriter())
    {
        
    }

    public override void Execute(CommandContext context)
    {
        OutputWriter.Write(Name);
    }
}