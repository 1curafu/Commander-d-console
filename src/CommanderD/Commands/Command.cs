using System.Security.AccessControl;
using CommanderD.Shell;

namespace CommanderD.Commands;

public abstract class Command
{
    public string Name { get; }
    public string Description { get; }

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
