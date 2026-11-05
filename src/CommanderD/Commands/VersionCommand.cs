using CommanderD.Shell;

namespace CommanderD.Commands;

public class VersionCommand : Command
{

    public VersionCommand(IOutputWriter outputWriter)
        :base("ver", "Shows the version number.", outputWriter)
    {
        
    }
    public override void Execute(CommandContext context)
    {
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        OutputWriter.WriteLn(versionString);
    }
}