using CommanderD.Filesystem;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Services;

public class FileSystemService : IFileSystemService
{
    private static readonly char[] InvalidNameCharacters = ['<', '>', ':', '"', '/', '\\', '|', '?', '*'];

    private readonly ShellContext _context;
    private readonly bool _saveOnChange;
    private string _currentDrive;
    private List<string> _currentSegments = new();

    public FileSystemService(ShellContext context, bool saveOnChange = false)
    {
        _context = context;
        _saveOnChange = saveOnChange;
        _currentDrive = context.Drives[0].Name;
    }

    public string CurrentDirectory => Build(_currentDrive, _currentSegments);

    public string ResolvePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        string text = path.Replace('/', '\\');
        string? driveName = null;

        if (text.Length >= 2 && char.IsAsciiLetter(text[0]) && text[1] == ':')
        {
            driveName = char.ToUpperInvariant(text[0]).ToString();
            text = text[2..];
        }

        string targetDrive = driveName ?? _currentDrive;
        var segments = new List<string>();

        bool isAbsolute = text.StartsWith('\\');
        if (!isAbsolute && string.Equals(targetDrive, _currentDrive, StringComparison.OrdinalIgnoreCase))
        {
            segments.AddRange(_currentSegments);
        }

        foreach (string part in text.Split('\\', StringSplitOptions.RemoveEmptyEntries))
        {
            if (part == ".")
            {
                continue;
            }

            if (part == "..")
            {
                if (segments.Count > 0)
                {
                    segments.RemoveAt(segments.Count - 1);
                }

                continue;
            }

            segments.Add(part);
        }

        return Build(targetDrive, segments);
    }

    public bool DirectoryExists(string path)
    {
        return Locate(path)?.Item is Folder;
    }

    public bool FileExists(string path)
    {
        return Locate(path)?.Item is FsFile;
    }

    public void ChangeDirectory(string path)
    {
        Located? located = Locate(path);

        if (located?.Item is not Folder folder)
        {
            throw new DirectoryNotFoundException($"The directory '{path}' does not exist.");
        }

        _currentDrive = located.Drive;
        _currentSegments = located.Segments;
        _context.CurrentFolder = folder;
    }

    public void CreateDirectory(string path)
    {
        (string driveName, List<string> segments) = Split(ResolvePath(path));

        Drive drive = FindDrive(driveName)
            ?? throw new DirectoryNotFoundException($"The drive '{driveName}:' does not exist.");

        Folder folder = drive.RootFolder;

        for (int i = 0; i < segments.Count; i++)
        {
            string name = segments[i];

            if (name.IndexOfAny(InvalidNameCharacters) >= 0)
            {
                throw new ArgumentException($"The name '{name}' is not valid.", nameof(path));
            }

            FileSystemItem? existing = FindChild(folder, name);

            if (existing is Folder existingFolder)
            {
                folder = existingFolder;
            }
            else if (existing != null)
            {
                throw new IOException($"'{name}' is a file.");
            }
            else
            {
                var created = new Folder(name, Build(drive.Name, segments.GetRange(0, i + 1)));
                folder.AddChildItem(created);
                folder = created;
            }
        }

        if (_saveOnChange)
        {
            _context.Save();
        }
    }

    public void DeleteFile(string path)
    {
        (string driveName, List<string> segments) = Split(ResolvePath(path));

        if (segments.Count == 0 || Locate(path)?.Item is not FsFile file)
        {
            throw new FileNotFoundException($"The file '{path}' does not exist.");
        }

        var parent = (Folder)Locate(Build(driveName, segments.GetRange(0, segments.Count - 1)))!.Item;
        parent.RemoveChildItem(file);

        if (_saveOnChange)
        {
            _context.Save();
        }
    }

    public void Rename(string path, string newName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newName);

        if (newName is "." or ".." || newName.IndexOfAny(InvalidNameCharacters) >= 0)
        {
            throw new ArgumentException($"The name '{newName}' is not valid.", nameof(newName));
        }

        (string driveName, List<string> segments) = Split(ResolvePath(path));
        Located? located = Locate(path);

        if (segments.Count == 0 || located == null)
        {
            throw new FileNotFoundException($"The file '{path}' does not exist.");
        }
        
        if (ContainsCurrentDirectory(driveName, segments))
        {
            throw new UnauthorizedAccessException($"'{path}' cannot be renamed.");
        }

        var parent = (Folder)Locate(Build(driveName, segments.GetRange(0, segments.Count - 1)))!.Item;
        FileSystemItem? existing = FindChild(parent, newName);

        if (existing != null && !ReferenceEquals(existing, located.Item))
        {
            throw new IOException($"A duplicate file name exists: '{newName}'.");
        }

        located.Item.Name = newName;

        List<string> newSegments = segments.GetRange(0, segments.Count - 1);
        newSegments.Add(newName);
        UpdatePath(located.Item, Build(driveName, newSegments));

        if (_saveOnChange)
        {
            _context.Save();
        }
    }

    public void Move(string source, string destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        (string sourceDrive, List<string> sourceSegments) = Split(ResolvePath(source));
        Located? located = Locate(source);

        if (sourceSegments.Count == 0 || located == null)
        {
            throw new FileNotFoundException($"The file '{source}' does not exist.");
        }

        if (ContainsCurrentDirectory(sourceDrive, sourceSegments))
        {
            throw new UnauthorizedAccessException($"'{source}' cannot be moved.");
        }

        string itemName = located.Segments[^1];
        Located? destinationLocated = Locate(destination);
        List<string> targetSegments;
        string targetDrive;

        if (destinationLocated?.Item is Folder)
        {
            targetDrive = destinationLocated.Drive;
            targetSegments = new List<string>(destinationLocated.Segments) { itemName };
        }
        else
        {
            (targetDrive, targetSegments) = Split(ResolvePath(destination));
        }

        if (targetSegments.Count == 0)
        {
            throw new ArgumentException("The destination is not valid.", nameof(destination));
        }

        string newName = targetSegments[^1];

        if (newName.IndexOfAny(InvalidNameCharacters) >= 0)
        {
            throw new ArgumentException($"The name '{newName}' is not valid.", nameof(destination));
        }

        bool sameDrive = string.Equals(sourceDrive, targetDrive, StringComparison.OrdinalIgnoreCase);

        if (located.Item is Folder && sameDrive && StartsWith(targetSegments, sourceSegments))
        {
            if (targetSegments.Count == sourceSegments.Count)
            {
                return;
            }

            throw new UnauthorizedAccessException($"'{source}' cannot be moved into itself.");
        }

        Located? targetParent = Locate(Build(targetDrive, targetSegments.GetRange(0, targetSegments.Count - 1)));

        if (targetParent?.Item is not Folder newParent)
        {
            throw new DirectoryNotFoundException($"The path '{destination}' does not exist.");
        }

        FileSystemItem? existing = FindChild(newParent, newName);

        if (existing != null && !ReferenceEquals(existing, located.Item))
        {
            throw new IOException($"A duplicate file name exists: '{newName}'.");
        }

        var oldParent = (Folder)Locate(Build(sourceDrive, sourceSegments.GetRange(0, sourceSegments.Count - 1)))!.Item;
        oldParent.RemoveChildItem(located.Item);
        located.Item.Name = newName;
        newParent.AddChildItem(located.Item);

        var newSegments = new List<string>(targetParent.Segments) { newName };
        UpdatePath(located.Item, Build(targetParent.Drive, newSegments));

        if (_saveOnChange)
        {
            _context.Save();
        }
    }

    public bool IsProtectedFile(string path)
    {
        return false;
    }

    public IReadOnlyList<DirectoryEntry> GetEntries(string path)
    {
        Located? located = Locate(path);

        switch (located?.Item)
        {
            case Folder folder:
                IEnumerable<DirectoryEntry> folders = folder.GetChildItems()
                    .OfType<Folder>()
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(item => new DirectoryEntry(item.Name, true, 0, item.CreatedOn, null));
                IEnumerable<DirectoryEntry> files = folder.GetChildItems()
                    .OfType<FsFile>()
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(item => new DirectoryEntry(item.Name, false, item.Size, item.CreatedOn, item.Content));
                return folders.Concat(files).ToList();

            case FsFile file:
                return [new DirectoryEntry(file.Name, false, file.Size, file.CreatedOn, file.Content)];

            default:
                throw new DirectoryNotFoundException($"The path '{path}' does not exist.");
        }
    }

    private sealed record Located(FileSystemItem Item, string Drive, List<string> Segments);

    private Located? Locate(string path)
    {
        (string driveName, List<string> segments) = Split(ResolvePath(path));

        Drive? drive = FindDrive(driveName);
        if (drive == null)
        {
            return null;
        }

        FileSystemItem item = drive.RootFolder;
        var canonical = new List<string>();

        foreach (string name in segments)
        {
            if (item is not Folder folder)
            {
                return null;
            }

            FileSystemItem? child = FindChild(folder, name);
            if (child == null)
            {
                return null;
            }

            canonical.Add(child.Name);
            item = child;
        }

        return new Located(item, drive.Name, canonical);
    }

    private bool ContainsCurrentDirectory(string driveName, List<string> segments)
    {
        if (!string.Equals(driveName, _currentDrive, StringComparison.OrdinalIgnoreCase)
            || segments.Count > _currentSegments.Count)
        {
            return false;
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (!string.Equals(segments[i], _currentSegments[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static bool StartsWith(List<string> segments, List<string> prefix)
    {
        if (prefix.Count > segments.Count)
        {
            return false;
        }

        for (int i = 0; i < prefix.Count; i++)
        {
            if (!string.Equals(segments[i], prefix[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static void UpdatePath(FileSystemItem item, string path)
    {
        item.Path = path;

        if (item is not Folder folder)
        {
            return;
        }

        foreach (FileSystemItem child in folder.GetChildItems())
        {
            UpdatePath(child, path.TrimEnd('\\') + "\\" + child.Name);
        }
    }

    private Drive? FindDrive(string name)
    {
        return _context.Drives.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static FileSystemItem? FindChild(Folder folder, string name)
    {
        return folder.GetChildItems().FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static string Build(string drive, IEnumerable<string> segments)
    {
        return $"{drive}:\\" + string.Join('\\', segments);
    }

    private static (string Drive, List<string> Segments) Split(string resolvedPath)
    {
        string drive = resolvedPath[..1];
        List<string> segments = resolvedPath[3..].Split('\\', StringSplitOptions.RemoveEmptyEntries).ToList();
        return (drive, segments);
    }
}