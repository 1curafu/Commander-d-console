namespace CommanderD.Commands;

public class CommandContext
{
    public string Name { get; }
    public string[] Arguments { get; }

    public CommandContext(string name, string[] arguments)
    {
        Name = name;
        Arguments = arguments;
    }
}
