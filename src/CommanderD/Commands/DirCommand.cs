using System;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

public class DirCommand : Command
{
  
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool GetVolumeInformation(
        string rootPathName,
        StringBuilder volumeNameBuffer, int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        StringBuilder fileSystemNameBuffer, int fileSystemNameSize);

    public DirCommand() { }

    public override void Execute(string param)
    {
        try
        {
            string path = string.IsNullOrWhiteSpace(param)
                ? Directory.GetCurrentDirectory()
                : param.Trim();

            var dir = new DirectoryInfo(path);
            if (!dir.Exists)
                throw new DirectoryNotFoundException();

            var drive = new DriveInfo(dir.Root.FullName);

         
            PrintVolumeHeader(drive);

            Console.WriteLine();
            Console.WriteLine($" Directory of {dir.FullName}");
            Console.WriteLine();

            int fileCount = 0, dirCount = 0;
            long totalSize = 0;


            if (dir.Parent != null)
            {
                PrintDirLine(dir.LastWriteTime, ".");
                PrintDirLine(dir.Parent.LastWriteTime, "..");
                dirCount += 2;
            }

            foreach (FileSystemInfo entry in dir.GetFileSystemInfos())
            {
               
                if ((entry.Attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0)
                    continue;

                if (entry is DirectoryInfo)
                {
                    PrintDirLine(entry.LastWriteTime, entry.Name);
                    dirCount++;
                }
                else if (entry is FileInfo file)
                {
                    Console.WriteLine($"{FormatDateTime(file.LastWriteTime)}{file.Length,18:N0} {file.Name}");
                    fileCount++;
                    totalSize += file.Length;
                }
            }

          
            Console.WriteLine($"{fileCount,16} File(s){totalSize,15:N0} bytes");
            Console.WriteLine($"{dirCount,16} Dir(s){drive.AvailableFreeSpace,16:N0} bytes free");
        }
        catch (DirectoryNotFoundException)
        {
            Console.WriteLine("The system cannot find the path specified.");
        }
        catch (UnauthorizedAccessException)
        {
            Console.WriteLine("Access is denied.");
        }
        catch (ArgumentException)
        {
            Console.WriteLine("The filename, directory name, or volume label syntax is incorrect.");
        }
        catch (IOException e)
        {
            Console.WriteLine(e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }
    }

    private static void PrintVolumeHeader(DriveInfo drive)
    {
        char letter = drive.Name[0];

        if (string.IsNullOrEmpty(drive.VolumeLabel))
            Console.WriteLine($" Volume in drive {letter} has no label.");
        else
            Console.WriteLine($" Volume in drive {letter} is {drive.VolumeLabel}");

        if (GetVolumeInformation(drive.RootDirectory.FullName, null, 0,
                out uint serial, out _, out _, null, 0))
        {
            Console.WriteLine($" Volume Serial Number is {serial >> 16:X4}-{serial & 0xFFFF:X4}");
        }
    }

    private static void PrintDirLine(DateTime time, string name)
    {
        Console.WriteLine($"{FormatDateTime(time)}    <DIR>          {name}");
    }

    
    private static string FormatDateTime(DateTime time)
    {
        var culture = CultureInfo.CurrentCulture;
        string date = time.ToString(culture.DateTimeFormat.ShortDatePattern, culture);
        string clock = time.ToString(culture.DateTimeFormat.ShortTimePattern, culture);
        return $"{date}  {clock}";
    }
}