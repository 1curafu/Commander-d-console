using System.Globalization;
using System.Text;
using CommanderD.Shell;

namespace CommanderD.Commands;

public class DirCommand : AbstractCommand
{
    public DirCommand(IOutputWriter outputWriter)
        : base("dir", "Zeigt die Dateien und Unterverzeichnisse eines Verzeichnisses an.", outputWriter)
    {
    }

    public override void Execute(CommandContext context)
    {
        
        string param = string.Join(" ", context.Arguments);
        string path = string.IsNullOrWhiteSpace(param)
            ? Directory.GetCurrentDirectory()
            : param.Trim('"');

        try
        {
            DirectoryInfo dir;
            DirectoryInfo[] dirs;
            FileInfo[] files;

            if (File.Exists(path))
            {
            
                var file = new FileInfo(path);
                dir = file.Directory!;
                dirs = [];
                files = [file];
            }
            else
            {
                dir = new DirectoryInfo(path);
                dirs = dir.GetDirectories();
                files = dir.GetFiles();
            }

            var output = new StringBuilder();
            output.AppendLine($" Verzeichnis von {dir.FullName}");
            output.AppendLine();

            int dirCount = 0, fileCount = 0;
            long totalSize = 0;

            var fmt = CultureInfo.CurrentCulture.DateTimeFormat;
            string dateTimePattern = $"{fmt.ShortDatePattern}  {fmt.ShortTimePattern}";

            foreach (var d in dirs)
            {
                output.AppendLine($"{d.LastWriteTime.ToString(dateTimePattern)}    <DIR>          {d.Name}");
                dirCount++;
            }

            foreach (var f in files)
            {
                output.AppendLine($"{f.LastWriteTime.ToString(dateTimePattern)}    {f.Length,14:N0} {f.Name}");
                fileCount++;
                totalSize += f.Length;
            }

            long freeBytes = new DriveInfo(dir.Root.FullName).AvailableFreeSpace;

            output.AppendLine($"{fileCount,16} Datei(en) {totalSize,14:N0} Bytes");
            output.Append($"{dirCount,16} Verzeichnis(se), {freeBytes,14:N0} Bytes frei");

            OutputWriter.WriteLn(output.ToString());
        }
        catch (DirectoryNotFoundException)
        {
            OutputWriter.WriteLn($"Das System kann den angegebenen Pfad nicht finden: {param}");
        }
        catch (UnauthorizedAccessException)
        {
            OutputWriter.WriteLn($"Zugriff verweigert: {param}");
        }
        catch (ArgumentException)
        {
            OutputWriter.WriteLn($"Ungültiger Pfad: {param}");
        }
        catch (IOException e)
        {
            OutputWriter.WriteLn($"Fehler beim Lesen des Verzeichnisses: {e.Message}");
        }
    }
}
