using CommanderD.Services;

namespace CommanderD.Tests.Fakes;

public class FakeFileSystemService : IFileSystemService
{
    private readonly HashSet<string> directories = new HashSet<string>(StringComparer.Ordinal);

    public string CurrentDirectory { get; set; }

    public string? ChangedTo { get; private set; }

    public int ChangeDirectoryCalls { get; private set; }

    public Exception? ResolvePathException { get; set; }

    public Exception? ChangeDirectoryException { get; set; }

    public Exception? CreateDirectoryException { get; set; }

    public int CreateDirectoryCalls { get; private set; }

    public FakeFileSystemService(string currentDirectory)
    {
        CurrentDirectory = currentDirectory;
        directories.Add(currentDirectory);
    }

    public void AddDirectory(string path)
    {
        directories.Add(path);
    }

    public string ResolvePath(string path)
    {
        if (ResolvePathException != null)
        {
            throw ResolvePathException;
        }

        return Path.GetFullPath(path, CurrentDirectory);
    }

    public bool DirectoryExists(string path)
    {
        return directories.Contains(path);
    }

    public void ChangeDirectory(string path)
    {
        ChangeDirectoryCalls++;

        if (ChangeDirectoryException != null)
        {
            throw ChangeDirectoryException;
        }

        ChangedTo = path;
        CurrentDirectory = path;
    }

    public void CreateDirectory(string path)
    {
        CreateDirectoryCalls++;

        if (CreateDirectoryException != null)
        {
            throw CreateDirectoryException;
        }

        directories.Add(path);
    }
}