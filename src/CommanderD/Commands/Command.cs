using System.Security.AccessControl;
using CommanderD.Shell;

namespace CommanderD.Commands;

public abstract class Command
{
    public string Name { get; }
    public string Description { get; }

    /// <summary>How the command is used. It is shown by HELP. Override it when the command takes arguments.</summary>
    public virtual string Usage => Name.ToUpperInvariant();
    
    public IOutputWriter OutputWriter { get; }

    protected Command(string name, string description, IOutputWriter outputWriter)
    {
        Name = name;
        Description = description;
        OutputWriter = outputWriter;
    }

    public abstract void Execute(CommandContext context);
}
