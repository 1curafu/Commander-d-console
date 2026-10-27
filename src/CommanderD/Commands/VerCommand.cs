namespace CommanderD.Commands;

public class VerCommand : AbstractCommand
{

    public VerCommand()
        :base("ver", "outputs the current OS version")
    {
        
    }
    public override CommandResult Execute(CommandContext context)
    {
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        return CommandResult.Ok(versionString);
    }
}