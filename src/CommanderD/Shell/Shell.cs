using CommanderD.Commands;
using CommanderD.Services;

namespace CommanderD.Shell;

public class Shell
{
    private readonly CommandInvoker _commandInvoker;
    private readonly IFileSystemService _fileSystemService;
    private bool _running;

    public Shell()
        : this(new CommandInvoker(new ConsoleOutputWriter()), new FileSystemService())
    {
    }

    public Shell(CommandInvoker commandInvoker)
        : this(commandInvoker, new FileSystemService())
    {
    }

    public Shell(CommandInvoker commandInvoker, IFileSystemService fileSystemService)
    {
        this._commandInvoker = commandInvoker;
        this._fileSystemService = fileSystemService;
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
