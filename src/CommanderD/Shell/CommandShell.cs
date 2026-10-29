using CommanderD.Commands;
using CommanderD.Services;

namespace CommanderD.Shell;

public class CommandShell
{
    private readonly CommandInvoker _commandInvoker;
    private readonly IFileSystemService _fileSystemService;

    public CommandShell(CommandInvoker commandInvoker)
        : this(commandInvoker, new FileSystemService())
    {
    }

    public CommandShell(CommandInvoker commandInvoker, IFileSystemService fileSystemService)
    {
        this._commandInvoker = commandInvoker;
        this._fileSystemService = fileSystemService;
    }

    public void Run()
    {
        bool running = true;

        while (running)
        {
            Console.Write(GetPrompt());
            string? input = Console.ReadLine();

            if (input == null)
            {
                break;
            }

            AbstractCommand? command = _commandInvoker.ExecuteCommand(input);

            if (command is ExitCommand { ShouldExit: true })
            {
                running = false;
            }
        }
    }

    private string GetPrompt()
    {
        return $"{_fileSystemService.CurrentDirectory}>";
    }
}
