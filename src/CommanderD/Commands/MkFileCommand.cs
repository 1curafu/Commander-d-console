using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class MkFileCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const string AccessDeniedMessage = "Access is denied.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public MkFileCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("mkfile", "Creates a file.", outputWriter)
    {
        _fileSystemService = fileSystemService;
        _outputWriter = outputWriter;
    }

    public override string Usage => "MKFILE <name> [content]\nMF <name> [content]";

    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length == 0)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
            return;
        }

        var name = context.Arguments[0];
        var content = string.Join(" ", context.Arguments.Skip(1));

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

            _fileSystemService.CreateFile(target, content);
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