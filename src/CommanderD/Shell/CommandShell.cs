using CommanderD.Services;

namespace CommanderD.Shell;

public class CommandShell
{
    private readonly CommandInvoker _commandInvoker;
    private readonly IFileSystemService _fileSystemService = new FileSystemService();

    public CommandShell(CommandInvoker commandInvoker)
    {
        this._commandInvoker = commandInvoker;
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

            _commandInvoker.ExecuteCommand(input);
        }
    }

    private string GetPrompt()
    {
        return $"{_fileSystemService.CurrentDirectory}>";
    }
}