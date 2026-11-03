namespace CommanderD.Filesystem;

public class Drive(string name, string label)
{
    public readonly Folder RootFolder = new("\\", $"{label}:\\", DateTime.Now);
    public readonly string Name = name;
    public readonly string Label = label;
}