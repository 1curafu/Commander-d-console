using System.Text;
using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class MoveCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const string NotFoundMessage = "The system cannot find the file specified.";
    private const string PathNotFoundMessage = "The system cannot find the path specified.";
    private const string DuplicateMessage = "A duplicate file name exists, or the file cannot be found.";
    private const string MultipleToFileMessage = "Cannot move multiple files to a single file.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public MoveCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("move", "Moves files or directories to a new location.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
    }

    public override string Usage => "MOVE <source>[,<source>...] <destination>\nMOVE <source> <source> ... <destination>";

    private enum MoveOutcome
    {
        Failed,
        File,
        Directory
    }

    public override void Execute(CommandContext context)
    {
        List<string> names = SplitNames(context.Arguments);

        if (names.Count < 2 || names.Any(name => name.Length == 0))
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string destinationName = names[^1];
        List<string> sources = names.GetRange(0, names.Count - 1);

        string destination;
        bool destinationIsDirectory;

        try
        {
            destination = _fileSystemService.ResolvePath(destinationName);
            destinationIsDirectory = _fileSystemService.DirectoryExists(destination);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        if (!destinationIsDirectory)
        {
            if (sources.Count > 1)
            {
                _outputWriter.WriteLn(MultipleToFileMessage);
                return;
            }

            if (destinationName.EndsWith('\\') || destinationName.EndsWith('/'))
            {
                _outputWriter.WriteLn(PathNotFoundMessage);
                return;
            }
        }

        int files = 0;
        int directories = 0;

        foreach (string source in sources)
        {
            switch (MoveOne(source, destination, destinationIsDirectory))
            {
                case MoveOutcome.File:
                    files++;
                    break;
                case MoveOutcome.Directory:
                    directories++;
                    break;
            }
        }

        if (files > 0)
        {
            _outputWriter.WriteLn($"{files,9} file(s) moved.");
        }

        if (directories > 0)
        {
            _outputWriter.WriteLn($"{directories,9} dir(s) moved.");
        }
    }

    private MoveOutcome MoveOne(string sourceName, string destination, bool destinationIsDirectory)
    {
        try
        {
            string source = _fileSystemService.ResolvePath(sourceName);
            bool isFile = _fileSystemService.FileExists(source);

            if (!isFile && !_fileSystemService.DirectoryExists(source))
            {
                _outputWriter.WriteLn(NotFoundMessage);
                return MoveOutcome.Failed;
            }

            if (_fileSystemService.IsProtectedFile(source)
                || (!destinationIsDirectory && _fileSystemService.IsProtectedFile(destination)))
            {
                _outputWriter.WriteLn(AccessDeniedMessage);
                return MoveOutcome.Failed;
            }

            _fileSystemService.Move(source, destination);
            return isFile ? MoveOutcome.File : MoveOutcome.Directory;
        }
        catch (UnauthorizedAccessException)
        {
            _outputWriter.WriteLn(AccessDeniedMessage);
        }
        catch (FileNotFoundException)
        {
            _outputWriter.WriteLn(NotFoundMessage);
        }
        catch (DirectoryNotFoundException)
        {
            _outputWriter.WriteLn(PathNotFoundMessage);
        }
        catch (IOException)
        {
            _outputWriter.WriteLn(DuplicateMessage);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
        }

        return MoveOutcome.Failed;
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
            else if ((c == ' ' || c == ',') && !inQuotes)
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