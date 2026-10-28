using CommanderD.Shell;

namespace CommanderD.Commands;

public class VerCommand : AbstractCommand
{

    public VerCommand()
        :base("ver", "outputs the current OS version")
    {
        
    }
    public override void Execute(CommandContext context)
    {
        var version = Environment.OSVersion;
        var versionString = version.ToString();
        OutputWriter.WriteLn(versionString);
    }
}