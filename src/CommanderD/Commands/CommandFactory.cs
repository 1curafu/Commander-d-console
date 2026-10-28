using CommanderD.Services;

namespace CommanderD.Commands;

public class CommandFactory
{
    private readonly Dictionary<string, Func<AbstractCommand>> commands =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "exit", () => new ExitCommand() },
            { "cd", () => new CdCommand(new FileSystemService()) },
            { "Ver", () => new VerCommand() },
        };

    public AbstractCommand? Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (commands.TryGetValue(name, out Func<AbstractCommand>? create))
        {
            return create();
        }

        return null;
    }

    public List<string> GetCommandNames()
    {
        return new List<string>(commands.Keys);
    }

    public void Register(string name, Func<AbstractCommand> create)
    {
        commands[name] = create;
    }
}