namespace CommanderD.Commands;

public class ShellContext
{
   
    private readonly PersistenceService _persistenceService;
    private readonly List<Drive> Drives { get; } = new();
    public Folder CurrentFolder { get; }
    public ConsoleColor BackgroundColor { get; set; } = ConsoleColor.Black;
    public ConsoleColor ForegroundColor { get; set; } = ConsoleColor.Gray;


    public ShellContext(PersistenceService persistenceService)
    {
        var drive = new Drive("C", "System");
        Drives.Add(drive);
        CurrentFolder = drive.RootFolder;
    }

    public void Load()
    {
        _persistenceService.Load(this);
    }

    public void Save()
    {
        _persistenceService.Save(this);
    }



}