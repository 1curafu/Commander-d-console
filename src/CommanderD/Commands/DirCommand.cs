using System;
using System.IO;

public class DirCommand : Command
{
    public DirCommand() { }

    public override void Execute(string param)
    {
        try
        {
            // Kein Parameter -> aktueller Ordner
            string path = string.IsNullOrWhiteSpace(param)
                ? Directory.GetCurrentDirectory()
                : param.Trim();

            var dir = new DirectoryInfo(path);
            Console.WriteLine($" Verzeichnis von {dir.FullName}");
            Console.WriteLine();

            int dirCount = 0, fileCount = 0;
            long totalSize = 0;

            foreach (var d in dir.GetDirectories())
            {
                Console.WriteLine($"{d.LastWriteTime:dd.MM.yyyy  HH:mm}    <DIR>          {d.Name}");
                dirCount++;
            }

            foreach (var f in dir.GetFiles())
            {
                Console.WriteLine($"{f.LastWriteTime:dd.MM.yyyy  HH:mm}    {f.Length,14:N0} {f.Name}");
                fileCount++;
                totalSize += f.Length;
            }

            Console.WriteLine($"{fileCount,16} Datei(en) {totalSize,14:N0} Bytes");
            Console.WriteLine($"{dirCount,16} Verzeichnis(se)");
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine($"Das System kann den angegebenen Pfad nicht finden: {param}");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine($"Zugriff verweigert: {param}");
        }
        catch (ArgumentException)
        {
            Console.WriteLine($"Ungültiger Pfad: {param}");
        }
        catch (IOException e)
        {
            Console.WriteLine($"Fehler beim Lesen des Verzeichnisses: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unerwarteter Fehler: {e.Message}");
        }
    }
}