using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class TypeCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter):Command("TYPE", "Displays file content", outputWriter)
{
    private const string SyntaxErrorMessage = "The syntax of the command is incorrect.\n";
    private const string FileNotFoundMessage = "The system cannot find the file specified.\n";
    
    public override void Execute(CommandContext context)
    {
        if (context.Arguments.Length != 1)
        {
            OutputWriter.Write(SyntaxErrorMessage);
            return;
        }
        var path = fileSystemService.ResolvePath(context.Arguments[0]);
        
        if (!fileSystemService.FileExists(path))
        {
            OutputWriter.Write(FileNotFoundMessage);
            return;
        }
        
        var file = fileSystemService.GetEntries(path)[0];
        
        if (file.Content == null)
        {
            OutputWriter.Write(SyntaxErrorMessage);
            return;
        }
      
        OutputWriter.Write(file.Content);
    }
}