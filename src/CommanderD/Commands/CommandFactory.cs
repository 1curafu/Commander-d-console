using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class CommandFactory
{
    private readonly Dictionary<string, Func<IOutputWriter, AbstractCommand>> commands =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "exit", (IOutputWriter outputWriter) => new ExitCommand(outputWriter) },
            { "cd", (IOutputWriter outputWriter) => new CdCommand(new FileSystemService(), outputWriter) },
            { "ver", (IOutputWriter outputWriter) => new VerCommand(outputWriter) },
        };

    public AbstractCommand? Create(string name, IOutputWriter outputWriter)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (commands.TryGetValue(name, out Func<IOutputWriter,AbstractCommand>? create))
        {
            return create(outputWriter);
        }

        return null;
    }

    public List<string> GetCommandNames()
    {
        return new List<string>(commands.Keys);
    }

    public void Register(string name, Func<IOutputWriter,AbstractCommand> create)
    {
        commands[name] = create;
    }
}