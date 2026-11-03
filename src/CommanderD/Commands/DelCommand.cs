using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class DelCommand : Command
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.";
    private const string AccessDeniedMessage = "Access is denied.";
    private const string FileInUseMessage = "The process cannot access the file because it is being used by another process.";

    private readonly IFileSystemService _fileSystemService;
    private readonly IOutputWriter _outputWriter;

    public DelCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("del", "Deletes a file.", outputWriter)
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

            if (!_fileSystemService.FileExists(target))
            {
                _outputWriter.WriteLn($"Could Not Find {target}");
                return;
            }

            if (_fileSystemService.IsProtectedFile(target))
            {
                _outputWriter.WriteLn(AccessDeniedMessage);
                return;
            }

            _fileSystemService.DeleteFile(target);
        }
        catch (UnauthorizedAccessException)
        {
            _outputWriter.WriteLn(AccessDeniedMessage);
        }
        catch (IOException)
        {
            _outputWriter.WriteLn(FileInUseMessage);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException)
        {
            _outputWriter.WriteLn(SyntaxErrorMessage);
        }
    }
}