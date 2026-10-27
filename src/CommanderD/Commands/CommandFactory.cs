namespace CommanderD.Commands;

public class CommandFactory
{
    private readonly Dictionary<string, Func<ICommand>> commands =
        new Dictionary<string, Func<ICommand>>(StringComparer.OrdinalIgnoreCase);

    public ICommand? Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (commands.TryGetValue(name, out Func<ICommand>? create))
        {
            return create();
        }

        return null;
    }

    public List<string> GetCommandNames()
    {
        return new List<string>(commands.Keys);
    }

    public void Register(string name, Func<ICommand> create)
    {
        commands[name] = create;
    }
}