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

    public void ChangeDirectory(string path)
    {
        Directory.SetCurrentDirectory(path);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }
}