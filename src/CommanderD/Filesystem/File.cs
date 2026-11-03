namespace CommanderD.Filesystem;

public class File(string name, string path, DateTime createdOn) : FileSystemItem(name, path, createdOn)
{
    private string _content { get; set; }
}