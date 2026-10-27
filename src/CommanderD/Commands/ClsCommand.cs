namespace CommanderD.Commands;

public class ClsCommand : AbstractCommand
{
    public ClsCommand()
        : base("cls", "clears the console")
    {
        
    }

    public override CommandResult Execute(CommandContext context)
    {
        Console.Clear();
        return CommandResult.Ok();
    }
}