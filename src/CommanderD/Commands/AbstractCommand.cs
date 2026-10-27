namespace CommanderD.Commands;

public abstract class AbstractCommand : ICommand
{
    public string Name { get; }
    public string Description { get; }

    protected AbstractCommand(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public abstract CommandResult Execute(CommandContext context);
}
