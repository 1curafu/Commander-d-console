namespace CommanderD.Filesystem;

public abstract class FileSystemItem(string name, string path)
{
    public string Name { get; set; } = name;
    public string Path { get; set; } = path;
    public DateTime CreatedOn { get; set; } = DateTime.Now;
}