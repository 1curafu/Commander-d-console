namespace CommanderD.Services;

public class FileSystemService : IFileSystemService
{
    public string CurrentDirectory => Directory.GetCurrentDirectory();

    public string ResolvePath(string path)
    {
        return Path.GetFullPath(path, CurrentDirectory);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public void ChangeDirectory(string path)
    {
        Directory.SetCurrentDirectory(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void DeleteFile(string path)
    {
        File.Delete(path);
    }

    public bool IsProtectedFile(string path)
    {
        if ((File.GetAttributes(path) & FileAttributes.System) != 0)
        {
            return true;
        }

        string windowsFolder = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

        return windowsFolder.Length > 0 && IsInside(path, windowsFolder);
    }

    private static bool IsInside(string path, string folder)
    {
        string prefix = Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder)) + Path.DirectorySeparatorChar;

        return Path.GetFullPath(path).StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }
}