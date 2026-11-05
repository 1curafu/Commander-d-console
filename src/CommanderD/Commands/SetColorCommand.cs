using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class SetColorCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const string BackgroundKeyword = "on";

    private readonly IConsoleService _consoleService;
    private readonly IOutputWriter _outputWriter;

    public SetColorCommand(IConsoleService consoleService, IOutputWriter outputWriter)
        : base("color", "Sets the default console foreground and background colors.", outputWriter)
    {
        _consoleService = consoleService;
        _outputWriter = outputWriter;
    }

    public override string Usage =>
        "COLOR [foreground [ON background]]\n" +
        "COLOR (without arguments) resets to the default colors.\n\n" +
        "Available colors:\n  " + string.Join(", ", ColorPalette.Names);

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            _consoleService.ResetColors();
            return;
        }

        if (!TryParseColors(context.Arguments, out ConsoleColor background, out ConsoleColor foreground))
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

    private bool TryParseColors(string[] arguments, out ConsoleColor background, out ConsoleColor foreground)
    {
        background = _consoleService.BackgroundColor;
        foreground = _consoleService.ForegroundColor;

        if (arguments.Length == 1)
        {
            return ColorPalette.TryGet(arguments[0], out foreground);
        }

        if (arguments.Length == 3 && string.Equals(arguments[1], BackgroundKeyword, StringComparison.OrdinalIgnoreCase))
        {
            return ColorPalette.TryGet(arguments[0], out foreground)
                   && ColorPalette.TryGet(arguments[2], out background);
        }

        return false;
    }
}