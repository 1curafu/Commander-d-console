namespace CommanderD.Shell;

public class CommandShell
{
    private readonly CommandInvoker commandInvoker;

    public CommandShell(CommandInvoker commandInvoker)
    {
        this.commandInvoker = commandInvoker;
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

            commandInvoker.ExecuteCommand(input);
        }
    }

    private string GetPrompt()
    {
        return ">";
    }
}