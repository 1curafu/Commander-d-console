using CommanderD.Services;

namespace CommanderD.Tests.Fakes;

public class FakeFileSystemService : IFileSystemService
{
    private readonly HashSet<string> directories = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> files = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> protectedFiles = new HashSet<string>(StringComparer.Ordinal);

    public string CurrentDirectory { get; set; }

    public string? ChangedTo { get; private set; }

    public int ChangeDirectoryCalls { get; private set; }

    public Exception? ResolvePathException { get; set; }

    public Exception? ChangeDirectoryException { get; set; }

    public Exception? CreateDirectoryException { get; set; }

    public int CreateDirectoryCalls { get; private set; }

    public Exception? DeleteFileException { get; set; }

    public int DeleteFileCalls { get; private set; }

    public Exception? RenameException { get; set; }

    public int RenameCalls { get; private set; }

    public string? RenamedTo { get; private set; }

    private static readonly char[] InvalidRenameCharacters = ['\\', '/', ':'];

    public FakeFileSystemService(string currentDirectory)
    {
        CurrentDirectory = currentDirectory;
        directories.Add(currentDirectory);
    }

    public void AddDirectory(string path)
    {
        directories.Add(path);
    }

    public void AddFile(string path)
    {
        files.Add(path);
    }

    public void AddProtectedFile(string path)
    {
        files.Add(path);
        protectedFiles.Add(path);
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

    public bool FileExists(string path)
    {
        return files.Contains(path);
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

    public void DeleteFile(string path)
    {
        DeleteFileCalls++;

        if (DeleteFileException != null)
        {
            throw DeleteFileException;
        }

        files.Remove(path);
    }

    public void Rename(string path, string newName)
    {
        RenameCalls++;

        if (RenameException != null)
        {
            throw RenameException;
        }

        if (newName.IndexOfAny(InvalidRenameCharacters) >= 0)
        {
            throw new ArgumentException($"The name '{newName}' is not valid.", nameof(newName));
        }

        string target = Path.Combine(Path.GetDirectoryName(path)!, newName);

        if (!string.Equals(path, target, StringComparison.OrdinalIgnoreCase)
            && (files.Contains(target) || directories.Contains(target)))
        {
            throw new IOException("A duplicate file name exists.");
        }

        if (files.Remove(path))
        {
            files.Add(target);
        }
        else if (directories.Remove(path))
        {
            directories.Add(target);
        }
        else
        {
            throw new FileNotFoundException(path);
        }

        RenamedTo = target;
    }

    public bool IsProtectedFile(string path)
    {
        return protectedFiles.Contains(path);
    }

    public IReadOnlyList<DirectoryEntry> GetEntries(string path)
    {
        if (!directories.Contains(path))
        {
            throw new DirectoryNotFoundException(path);
        }

        return new List<DirectoryEntry>();
    }
}