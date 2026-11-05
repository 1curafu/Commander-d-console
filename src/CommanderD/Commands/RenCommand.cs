using System.Text;
using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class RenCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const string NotFoundMessage = "The system cannot find the file specified.";
    private const string DuplicateMessage = "A duplicate file name exists, or the file cannot be found.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public RenCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("ren", "Renames a file or a directory.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
    }

    public override void Execute(CommandContext context)
    {
        List<string> names = SplitNames(context.Arguments);

        if (names.Count != 2 || names[0].Length == 0 || names[1].Length == 0)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        try
        {
            string source = _fileSystemService.ResolvePath(names[0]);

            if (!_fileSystemService.FileExists(source) && !_fileSystemService.DirectoryExists(source))
            {
                _outputWriter.WriteLn(NotFoundMessage);
                return;
            }

            if (_fileSystemService.IsProtectedFile(source))
            {
                _outputWriter.WriteLn(AccessDeniedMessage);
                return;
            }

            _fileSystemService.Rename(source, names[1]);
        }
        catch (UnauthorizedAccessException)
        {
            _outputWriter.WriteLn(AccessDeniedMessage);
        }
        catch (Exception ex) when (ex is FileNotFoundException or DirectoryNotFoundException)
        {
            _outputWriter.WriteLn(NotFoundMessage);
        }
        catch (IOException)
        {
            _outputWriter.WriteLn(DuplicateMessage);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
        }
    }
    
    private static List<string> SplitNames(string[] arguments)
    {
        var names = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;
        bool hasName = false;

        foreach (char c in string.Join(" ", arguments))
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                hasName = true;
            }
            else if (c == ' ' && !inQuotes)
            {
                if (hasName)
                {
                    names.Add(current.ToString());
                    current.Clear();
                    hasName = false;
                }
            }
            else
            {
                current.Append(c);
                hasName = true;
            }
        }

        if (hasName)
        {
            names.Add(current.ToString());
        }

        return names;
    }
}