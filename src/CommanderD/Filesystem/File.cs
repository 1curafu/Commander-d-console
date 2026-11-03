namespace CommanderD.Filesystem;

public class File(string name, string path) : FileSystemItem(name, path)
{
    private string _content { get; set; }
}