using CommanderD.Services;

namespace CommanderD.Filesystem;

public class ShellContext
{
    private readonly PersistenceService _persistenceService;

    public List<Drive> Drives { get; } = new();
    public Folder CurrentFolder { get; set; }
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;

    public ShellContext(PersistenceService persistenceService)
    {
        _persistenceService = persistenceService;

        var drive = new Drive("C", "System");
        Drives.Add(drive);
        CurrentFolder = drive.RootFolder;
    }

    public void Load()
    {
        List<Drive> drives = _persistenceService.LoadDrives();

        if (drives.Count == 0)
        {
            return;
        }

        Drives.Clear();
        Drives.AddRange(drives);
        CurrentFolder = Drives[0].RootFolder;
    }

    public void Save()
    {
        _persistenceService.SaveDrives(Drives);
    }
}
