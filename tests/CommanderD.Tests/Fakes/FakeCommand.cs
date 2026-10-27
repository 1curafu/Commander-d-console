using CommanderD.Commands;

namespace CommanderD.Tests.Fakes;

public class FakeCommand : AbstractCommand
{
    private readonly CommandResult result;

    public CommandContext? LastContext { get; private set; }

    public FakeCommand(string name = "fake", CommandResult? result = null)
        : base(name, "fake command")
    {
        this.result = result ?? CommandResult.Ok("fake output");
    }

    public override CommandResult Execute(CommandContext context)
    {
        LastContext = context;
        return result;
    }
}