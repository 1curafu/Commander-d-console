using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class CdCommand : AbstractCommand
{
    private const string PathNotFoundMessage = "The system cannot find the path specified.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public CdCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("cd", "Displays or changes the current directory.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            _outputWriter.WriteLn(_fileSystemService.CurrentDirectory);
        }

        string path = string.Join(" ", context.Arguments).Trim('"');

        try
        {
            string target = _fileSystemService.ResolvePath(path);

            if (!_fileSystemService.DirectoryExists(target) && !_fileSystemService.FileExists(target)) 
            {
                _outputWriter.WriteLn(PathNotFoundMessage);
            }
            else
            {
                _fileSystemService.ChangeDirectory(target);
            }
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or ArgumentException
                                   or NotSupportedException)
        {
            _outputWriter.WriteLn(PathNotFoundMessage);
        }
    }
}
