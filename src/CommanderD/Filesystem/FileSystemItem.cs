namespace CommanderD.Filesystem;

public abstract class FileSystemItem(string name, string path)
{
    public string Name { get; private set; } = name;
    public string Path { get; private set; } = path;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}
