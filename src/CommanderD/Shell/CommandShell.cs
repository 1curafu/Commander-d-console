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

            running = ProcessInput(input);
        }
    }

    public bool ProcessInput(string input)
    {
        var result = commandInvoker.ExecuteCommand(input);

        if (result.OutputMessage != "")
        {
            Console.WriteLine(result.OutputMessage);
        }

        return !result.ShouldExit;
    }

    private string GetPrompt()
    {
        return ">";
    }
}