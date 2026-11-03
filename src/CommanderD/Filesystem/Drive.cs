namespace CommanderD.Filesystem;

public class Drive(string name, string label)
{
    public readonly Folder RootFolder = new("\\", $"{name}:\\");
    public readonly string Name = name;
    public readonly string Label = label;
}