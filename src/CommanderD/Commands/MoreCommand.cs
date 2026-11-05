using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;


public class MoreCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const int DefaultWindowHeight = 25;

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;
    private readonly Func<int> _windowHeight;
    private readonly Func<ConsoleKeyInfo> _readKey;

    public MoreCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : this(fileSystemService, outputWriter, GetConsoleWindowHeight, ReadConsoleKey)
    {
    }

    
    public MoreCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter,
        Func<int> windowHeight, Func<ConsoleKeyInfo> readKey)
        : base("more", "Displays output one screen at a time.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
        _windowHeight = windowHeight;
        _readKey = readKey;
    }

    public override void Execute(CommandContext context)
    {
        string name = string.Join(" ", context.Arguments).Trim('"');

        if (name.Length == 0)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string target;

        try
        {
            target = _fileSystemService.ResolvePath(name);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string? content = ReadContent(target);

        if (content == null)
        {
            _outputWriter.WriteLn($"Cannot access file {target}");
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
      
        int pageSize = Math.Max(1, _windowHeight() - 1);
        int shown = ShowLines(lines, 0, pageSize);

        while (shown < lines.Length)
        {
            string status = $"-- More ({shown * 100 / lines.Length}%) --";
            _outputWriter.Write(status);

            ConsoleKeyInfo key = _readKey();

          
            _outputWriter.Write("\r" + new string(' ', status.Length) + "\r");

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
            _outputWriter.WriteLn(lines[i]);
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

    private static int GetConsoleWindowHeight()
    {
        try
        {
            return Console.WindowHeight > 0 ? Console.WindowHeight : DefaultWindowHeight;
        }
        catch (IOException)
        {
           
            return DefaultWindowHeight;
        }
    }

    private static ConsoleKeyInfo ReadConsoleKey()
    {
        if (Console.IsInputRedirected)
        {
            
            return new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false);
        }

        
        bool previous = Console.TreatControlCAsInput;
        Console.TreatControlCAsInput = true;

        try
        {
            return Console.ReadKey(intercept: true);
        }
        finally
        {
            Console.TreatControlCAsInput = previous;
        }
    }
}
