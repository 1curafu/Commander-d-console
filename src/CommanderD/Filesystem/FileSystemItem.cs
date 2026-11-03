namespace CommanderD.Filesystem;

public abstract class FileSystemItem(string name, string path, DateTime createdOn)
{
    private string Name { get; set; } = name;
    private string Path { get; set; } = path;
    private DateTime CreatedOn { get; set; } = createdOn;
}