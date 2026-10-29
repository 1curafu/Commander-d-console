using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class SetColorCommand : AbstractCommand
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";

    private readonly IConsoleService _consoleService;
    private readonly IOutputWriter _outputWriter;

    public SetColorCommand(IConsoleService consoleService, IOutputWriter outputWriter)
        : base("color", "Sets the default console foreground and background colors.", outputWriter)
    {
        _consoleService = consoleService;
        _outputWriter = outputWriter;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            _consoleService.ResetColors();
            return;
        }

        if (context.Arguments.Length > 1 || !TryParseColors(context.Arguments[0], out ConsoleColor background, out ConsoleColor foreground))
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        if (background == foreground)
        {
            return;
        }

        _consoleService.SetColors(background, foreground);
    }

    private bool TryParseColors(string code, out ConsoleColor background, out ConsoleColor foreground)
    {
        background = _consoleService.BackgroundColor;
        foreground = _consoleService.ForegroundColor;

        if (code.Length is < 1 or > 2 || !code.All(Uri.IsHexDigit))
        {
            return false;
        }

        if (code.Length == 2)
        {
            background = (ConsoleColor)Convert.ToInt32(code[0].ToString(), 16);
        }

        foreground = (ConsoleColor)Convert.ToInt32(code[^1].ToString(), 16);
        return true;
    }
}