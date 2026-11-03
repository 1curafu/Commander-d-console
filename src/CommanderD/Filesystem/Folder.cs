namespace CommanderD.Filesystem;

public class Folder(string name, string path, DateTime createdOn) : FileSystemItem(name, path, createdOn)
{
    private readonly List<FileSystemItem> ChildItems = new();

    public void AddChildItem(FileSystemItem item)
    {
        ChildItems.Add(item);
    }
    public void RemoveChildItem(FileSystemItem item)
    {
        ChildItems.Remove(item);
    }
    public List<FileSystemItem> GetChildItems()
    {
        return ChildItems;
    }
    
}