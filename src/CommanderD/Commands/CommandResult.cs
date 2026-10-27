namespace CommanderD.Commands;

public class CommandResult
{
    public bool Success { get; }
    public string OutputMessage { get; }
    public bool ShouldExit { get; }

    private CommandResult(bool success, string outputMessage, bool shouldExit)
    {
        Success = success;
        OutputMessage = outputMessage;
        ShouldExit = shouldExit;
    }

    public static CommandResult Ok(string outputMessage = "")
    {
        return new CommandResult(true, outputMessage, false);
    }

    public static CommandResult Fail(string outputMessage)
    {
        return new CommandResult(false, outputMessage, false);
    }

    public static CommandResult Exit()
    {
        return new CommandResult(true, "", true);
    }
}
