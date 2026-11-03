using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class CommandFactory
{
    private readonly Dictionary<string, Func<IOutputWriter, Command>> commands =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "exit", (IOutputWriter outputWriter) => new ExitCommand(outputWriter) },
            { "cd", (IOutputWriter outputWriter) => new CdCommand(new FileSystemService(), outputWriter) },
            { "ver", (IOutputWriter outputWriter) => new VersionCommand(outputWriter) },
            { "mkdir", (IOutputWriter outputWriter) => new MkDirCommand(new FileSystemService(), outputWriter) },
            { "md", (IOutputWriter outputWriter) => new MkDirCommand(new FileSystemService(), outputWriter) },
            { "dir", (IOutputWriter outputWriter) => new DirCommand(outputWriter) },
            { "color", (IOutputWriter outputWriter) => new SetColorCommand(new ConsoleService(), outputWriter) },
        };

    public Command? Create(string name, IOutputWriter outputWriter)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        if (commands.TryGetValue(name, out Func<IOutputWriter,Command>? create))
        {
            return create(outputWriter);
        }

        return null;
    }

    public IList<string> GetCommands()
    {
        return new List<string>(commands.Keys);
    }

    public void Register(string name, Func<IOutputWriter,Command> create)
    {
        commands[name] = create;
    }
}