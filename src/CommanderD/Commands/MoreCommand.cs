using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class MoreCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IConsoleService _consoleService;

    public MoreCommand(IFileSystemService fileSystemService, IConsoleService consoleService, IOutputWriter outputWriter)
        : base("more", "Displays output one screen at a time.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _consoleService = consoleService;
    }

    public override string Usage => "MORE <file>";

    public override void Execute(CommandContext context)
    {
        string name = string.Join(" ", context.Arguments).Trim('"');

        if (name.Length == 0)
        {
            OutputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string target;

        try
        {
            target = _fileSystemService.ResolvePath(name);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            OutputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string? content = ReadContent(target);

        if (content == null)
        {
            OutputWriter.WriteLn($"Cannot access file {target}");
            return;
        }

        ShowPaged(SplitLines(content));
    }

    private string? ReadContent(string target)
    {
        if (!_fileSystemService.FileExists(target))
        {
            return null;
        }

        try
        {
            IReadOnlyList<DirectoryEntry> entries = _fileSystemService.GetEntries(target);
            return entries.Count == 1 ? entries[0].Content ?? string.Empty : null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    private void ShowPaged(string[] lines)
    {
       
        int pageSize = Math.Max(1, _consoleService.WindowHeight - 1);
        int shown = ShowLines(lines, 0, pageSize);

        while (shown < lines.Length)
        {
            string status = $"-- More ({shown * 100 / lines.Length}%) --";
            OutputWriter.Write(status);

            ConsoleKeyInfo key = _consoleService.ReadKey();

            
            OutputWriter.Write("\r" + new string(' ', status.Length) + "\r");

            if (IsQuit(key))
            {
                return;
            }

            if (key.Key == ConsoleKey.Spacebar)
            {
                shown = ShowLines(lines, shown, pageSize);
            }
            else if (key.Key == ConsoleKey.Enter)
            {
                shown = ShowLines(lines, shown, 1);
            }
        }
    }

    private int ShowLines(string[] lines, int start, int count)
    {
        int end = Math.Min(lines.Length, start + count);

        for (int i = start; i < end; i++)
        {
            OutputWriter.WriteLn(lines[i]);
        }

        return end;
    }

    private static bool IsQuit(ConsoleKeyInfo key)
    {
        bool ctrlC = key.Key == ConsoleKey.C && key.Modifiers.HasFlag(ConsoleModifiers.Control);
        return key.Key == ConsoleKey.Q || ctrlC;
    }

    private static string[] SplitLines(string content)
    {
        if (content.Length == 0)
        {
            return [];
        }

        string[] lines = content.Replace("\r\n", "\n").Split('\n');

      
        return lines[^1].Length == 0 ? lines[..^1] : lines;
    }
}
