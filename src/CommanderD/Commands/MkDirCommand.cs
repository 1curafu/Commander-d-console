using CommanderD.Services;

namespace CommanderD.Commands
{
    public class MkDirCommand : AbstractCommand
    {
        private readonly IFileSystemService _fileSystemService;

        public MkDirCommand(IFileSystemService fileSystemService) 
            : base("mkdir", "Creates a directory.")
        {
            _fileSystemService = fileSystemService;
        }

        public override CommandResult Execute(CommandContext context)
        {
            if (context.Arguments == null || context.Arguments.Count == 0)
            {
                return CommandResult.Fail("The syntax of the command is incorrect.");
            }

            var dirName = context.Arguments[0];

            try
            {
                if (_fileSystemService.DirectoryExists(dirName))
                {
                    return CommandResult.Fail($"A subdirectory or file {dirName} already exists.");
                }

                _fileSystemService.CreateDirectory(dirName);
                return CommandResult.Ok(string.Empty);
            }
            catch
            {
                return CommandResult.Fail("The system cannot find the path specified.");
            }
        }
    }
}