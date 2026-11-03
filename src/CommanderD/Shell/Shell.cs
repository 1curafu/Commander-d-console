using CommanderD.Commands;
using CommanderD.Filesystem;
using CommanderD.Services;

namespace CommanderD.Shell;

public class Shell
{
    private readonly CommandInvoker _commandInvoker;
    private readonly IFileSystemService _fileSystemService;
    private bool _running;

    public Shell()
        : this(CreateDefault())
    {
    }

    public Shell(CommandInvoker commandInvoker)
        : this(commandInvoker, new FileSystemService(new ShellContext(new PersistenceService())))
    {
    }

    private Shell((CommandInvoker Invoker, IFileSystemService FileSystem) setup)
        : this(setup.Invoker, setup.FileSystem)
    {
    }

    public Shell(CommandInvoker commandInvoker, IFileSystemService fileSystemService)
    {
        this._commandInvoker = commandInvoker;
        this._fileSystemService = fileSystemService;
    }

    private static (CommandInvoker, IFileSystemService) CreateDefault()
    {
        var outputWriter = new ConsoleOutputWriter();
        var context = new ShellContext(new PersistenceService());

        context.Load();

        var fileSystem = new FileSystemService(context, saveOnChange: true);
        var factory = new CommandFactory(fileSystem, new ConsoleService());

        return (new CommandInvoker(new Parser(), factory, outputWriter), fileSystem);
    }

    public void Run()
    {
        _running = true;

        while (_running)
        {
            string? input = ReadInput(DisplayPrompt());

            if (input == null)
            {
                break;
            }

            ProcessInput(input);
        }
    }

    private string DisplayPrompt()
    {
        return $"{_fileSystemService.CurrentDirectory}>";
    }

    private string? ReadInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }

    private void ProcessInput(string input)
    {
        Command? command = _commandInvoker.ExecuteCommand(input);

        if (command is ExitCommand { ShouldExit: true })
        {
            _running = false;
        }
    }
}
