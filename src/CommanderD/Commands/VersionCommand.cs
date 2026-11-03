using CommanderD.Shell;

namespace CommanderD.Commands;

public class VersionCommand : Command
{

    public VersionCommand(IOutputWriter outputWriter)
        :base("ver", "outputs the current OS version", outputWriter)
    {
        
    }
    public override void Execute(CommandContext context)
    {
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        OutputWriter.WriteLn(versionString);
    }
}