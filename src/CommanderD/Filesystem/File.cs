namespace CommanderD.Filesystem;

public class File(string name, string path) : FileSystemItem(name, path)
{
    public string _content { get; set; }
}