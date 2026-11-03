using CommanderD.Filesystem;
using FsFile = CommanderD.Filesystem.File;

namespace CommanderD.Services;

/// <summary>
/// The file system of the shell (implements <see cref="IFileSystemService"/>). It only works on the virtual drives of the <see cref="ShellContext"/>
/// and never touches the real disk. Paths look like in the Windows command prompt: <c>C:\docs</c>.
/// </summary>
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
                    .Select(item => new DirectoryEntry(item.Name, true, 0, item.CreatedOn));
                IEnumerable<DirectoryEntry> files = folder.GetChildItems()
                    .OfType<FsFile>()
                    .OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(item => new DirectoryEntry(item.Name, false, item.Size, item.CreatedOn));
                return folders.Concat(files).ToList();

            case FsFile file:
                return [new DirectoryEntry(file.Name, false, file.Size, file.CreatedOn)];

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
