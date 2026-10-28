using System.Security.AccessControl;
using CommanderD.Shell;

namespace CommanderD.Commands;

public abstract class AbstractCommand
{
    public string Name { get; }
    public string Description { get; }
    
    public IOutputWriter OutputWriter { get; }

    protected AbstractCommand(string name, string description, IOutputWriter outputWriter)
    {
        Name = name;
        Description = description;
        OutputWriter = outputWriter;
    }

    public abstract void Execute(CommandContext context);
}
