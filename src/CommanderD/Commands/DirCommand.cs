using System.Globalization;
using System.Text;
using CommanderD.Services;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class DirCommand : Command
{
    private readonly IFileSystemService _fileSystemService;

    public DirCommand(IFileSystemService fileSystemService, IOutputWriter outputWriter)
        : base("dir", "Displays a list of files and subdirectories in a directory.", outputWriter)
    {
        _fileSystemService = fileSystemService;
    }

    public override void Execute(CommandContext context)
    {
        string param = string.Join(" ", context.Arguments);

        try
        {
            string target = string.IsNullOrWhiteSpace(param)
                ? _fileSystemService.CurrentDirectory
                : _fileSystemService.ResolvePath(param.Trim('"'));

            IReadOnlyList<DirectoryEntry> entries = _fileSystemService.GetEntries(target);
            string directory = _fileSystemService.DirectoryExists(target) ? target : GetParent(target);

            var output = new StringBuilder();
            output.AppendLine($" Directory of {directory}");
            output.AppendLine();

            int dirCount = 0, fileCount = 0;
            long totalSize = 0;

            DateTimeFormatInfo fmt = CultureInfo.CurrentCulture.DateTimeFormat;
            string dateTimePattern = $"{fmt.ShortDatePattern}  {fmt.ShortTimePattern}";

            foreach (DirectoryEntry entry in entries.Where(e => e.IsDirectory))
            {
                output.AppendLine($"{entry.CreatedOn.ToString(dateTimePattern)}    <DIR>          {entry.Name}");
                dirCount++;
            }

            foreach (DirectoryEntry entry in entries.Where(e => !e.IsDirectory))
            {
                output.AppendLine($"{entry.CreatedOn.ToString(dateTimePattern)}    {entry.Size,14:N0} {entry.Name}");
                fileCount++;
                totalSize += entry.Size;
            }

            output.AppendLine($"{fileCount,16} File(s) {totalSize,14:N0} bytes");
            output.Append($"{dirCount,16} Dir(s)");

            OutputWriter.WriteLn(output.ToString());
        }
        catch (DirectoryNotFoundException)
        {
            OutputWriter.WriteLn("The system cannot find the path specified.");
        }
        catch (UnauthorizedAccessException)
        {
            OutputWriter.WriteLn("Access is denied.");
        }
        catch (ArgumentException)
        {
            OutputWriter.WriteLn("The filename, directory name, or volume label syntax is incorrect.");
        }
        catch (IOException e)
        {
            OutputWriter.WriteLn($"Error reading the directory: {e.Message}");
        }
    }

    private static string GetParent(string path)
    {
        int index = path.LastIndexOf('\\');

        return index <= 2 ? path[..3] : path[..index];
    }
}
