using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class MkDirCommand : AbstractCommand
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public MkDirCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("mkdir", "Creates a directory.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
    }

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        string name = string.Join(" ", context.Arguments).Trim('"');

        if (name.Length == 0)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        try
        {
            string target = _fileSystemService.ResolvePath(name);

            if (_fileSystemService.DirectoryExists(target) || _fileSystemService.FileExists(target))
            {
                _outputWriter.WriteLn($"A subdirectory or file {name} already exists.");
                return;
            }

            _fileSystemService.CreateDirectory(target);
        }
        catch (UnauthorizedAccessException)
        {
            _outputWriter.WriteLn(AccessDeniedMessage);
        }
        catch (Exception ex) when (ex is IOException
                                       or ArgumentException
                                       or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
        }
    }
}