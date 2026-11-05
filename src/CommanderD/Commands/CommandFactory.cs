using CommanderD.Filesystem;
using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class CommandFactory
{
    private readonly Dictionary<string, Func<IOutputWriter, Command>> commands;

    public CommandFactory()
        : this(new FileSystemService(new ShellContext(new PersistenceService())), new ConsoleService())
    {
    }

    public CommandFactory(IFileSystemService fileSystemService, IConsoleService consoleService)
    {
        commands = new Dictionary<string, Func<IOutputWriter, Command>>(StringComparer.OrdinalIgnoreCase)
        {
            { "exit", outputWriter => new ExitCommand(outputWriter) },
            { "cd", outputWriter => new CdCommand(fileSystemService, outputWriter) },
            { "ver", outputWriter => new VersionCommand(outputWriter) },
            { "mkdir", outputWriter => new MkDirCommand(fileSystemService, outputWriter) },
            { "md", outputWriter => new MkDirCommand(fileSystemService, outputWriter) },
            { "dir", outputWriter => new DirCommand(fileSystemService, outputWriter) },
            { "del", outputWriter => new DelCommand(fileSystemService, outputWriter) },
            { "color", outputWriter => new SetColorCommand(consoleService, outputWriter) },
            { "cls", outputWriter => new ClsCommand(consoleService, outputWriter) },
            { "clear", outputWriter => new ClsCommand(consoleService, outputWriter) },
            { "help", outputWriter => new HelpCommand(this, outputWriter) },
            { "type", outputWriter => new TypeCommand(fileSystemService, outputWriter) },
            { "ren", outputWriter => new RenCommand(fileSystemService, outputWriter) },
            { "rename", outputWriter => new RenCommand(fileSystemService, outputWriter) },
            { "more", outputWriter => new MoreCommand(fileSystemService, outputWriter) },
        };
    }

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