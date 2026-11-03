namespace CommanderD.Filesystem;

public class Folder(string name, string path) : FileSystemItem(name, path)
{
    private readonly List<FileSystemItem> _childItems = new();

    public void AddChildItem(FileSystemItem item)
    {
        _childItems.Add(item);
    }
    public void RemoveChildItem(FileSystemItem item)
    {
        _childItems.Remove(item);
    }
    public List<FileSystemItem> GetChildItems()
    {
        return _childItems;
    }
    
}