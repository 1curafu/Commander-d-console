using System.Text;

namespace CommanderD.Commands;

public class DirCommand : AbstractCommand
{
    public DirCommand() : base("dir", "Zeigt die Dateien und Unterverzeichnisse eines Verzeichnisses an.")
    {
    }

    public override CommandResult Execute(CommandContext context)
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

            foreach (var d in dirs)
            {
                output.AppendLine($"{d.LastWriteTime:dd.MM.yyyy  HH:mm}    <DIR>          {d.Name}");
                dirCount++;
            }

            foreach (var f in files)
            {
                output.AppendLine($"{f.LastWriteTime:dd.MM.yyyy  HH:mm}    {f.Length,14:N0} {f.Name}");
                fileCount++;
                totalSize += f.Length;
            }

            output.AppendLine($"{fileCount,16} Datei(en) {totalSize,14:N0} Bytes");
            output.Append($"{dirCount,16} Verzeichnis(se)");

            return CommandResult.Ok(output.ToString());
        }
        catch (DirectoryNotFoundException)
        {
            return CommandResult.Fail($"Das System kann den angegebenen Pfad nicht finden: {param}");
        }
        catch (UnauthorizedAccessException)
        {
            return CommandResult.Fail($"Zugriff verweigert: {param}");
        }
        catch (ArgumentException)
        {
            return CommandResult.Fail($"Ungültiger Pfad: {param}");
        }
        catch (IOException e)
        {
            return CommandResult.Fail($"Fehler beim Lesen des Verzeichnisses: {e.Message}");
        }
    }
}
